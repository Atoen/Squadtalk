#!lua name=user_connections

--!strict

local CONNECTION_KEY = 'user:connections:'
local CURRENT_USER_STATUS_KEY = 'user:status'
local SELECTED_USER_STATUS_KEY = 'user:set_status'

-- enum UserStatus
-- {
--     Unknown = 0,
--     Online = 1,
--     Away = 2,
--     DoNotDisturb = 3,
--     Offline = 4
-- }

local ONLINE = 1
local OFFLINE = 4

-- For sending redis response
local TRUE = 1
local FALSE = 0

-- If user has set custom status, reuse it
-- Online and Away (caused by inactivity only) are overriden by default
local function connection_started(_, args)
    local userId = args[1]
    local connectionId = args[2]

    local userConnectionsKey = CONNECTION_KEY..userId

    local existingConnectionCount = redis.call('SCARD', userConnectionsKey)
    redis.call('SADD', userConnectionsKey, connectionId)

    local selectedStatus = tonumber(redis.call('HGET', SELECTED_USER_STATUS_KEY, userId) or -1)
    local statusChanged = FALSE

    -- Current connection is the only one
    -- Add user current status, if it is not selected as 'Offline'
    if existingConnectionCount == 0 then
        if selectedStatus == -1 then
            selectedStatus = ONLINE
            statusChanged = TRUE
            redis.call('HSET', CURRENT_USER_STATUS_KEY, userId, selectedStatus)
        end
        if selectedStatus ~= OFFLINE then
            statusChanged = TRUE
            redis.call('HSET', CURRENT_USER_STATUS_KEY, userId, selectedStatus)
        end
    end

    return { statusChanged, selectedStatus }
end

local function connection_ended(_, args)
    local userId = args[1]
    local connectionId = args[2]

    local userConnectionsKey = CONNECTION_KEY..userId
    redis.call('SREM', userConnectionsKey, connectionId)
    local remainingConnectionCount = redis.call('SCARD', userConnectionsKey)

    local currentStatus = tonumber(redis.call('HGET', SELECTED_USER_STATUS_KEY, userId) or -1)

    if remainingConnectionCount > 0 or currentStatus == OFFLINE then
        -- No visible change in status
        return {FALSE, currentStatus}
    end

    -- Preserve manually set status that is not 'Online'
    if currentStatus == ONLINE then
        redis.call('HDEL', SELECTED_USER_STATUS_KEY, userId)
    end

    redis.call('HDEL', CURRENT_USER_STATUS_KEY, userId)

    return {TRUE, OFFLINE}
end

local function set_user_status(_, args)
    local userId = args[1]
    local newStatus = tonumber(args[2])

    local currentStatus = tonumber(redis.call('HGET', CURRENT_USER_STATUS_KEY, userId) or -1)
    if (currentStatus == newStatus) then
        return {FALSE, currentStatus}
    end

    redis.call('HSET', CURRENT_USER_STATUS_KEY, userId, newStatus)
    if newStatus == ONLINE then
        redis.call('HDEL', SELECTED_USER_STATUS_KEY, userId)
    else
        redis.call('HSET', SELECTED_USER_STATUS_KEY, userId, newStatus)
    end

    return {TRUE, newStatus}
end

local function clear_connections(_, _)
    local connectionKeys = redis.call('KEYS', CONNECTION_KEY .. '*')

    for _, key in ipairs(connectionKeys) do
        redis.call('DEL', key)
    end

    redis.call('DEL', CURRENT_USER_STATUS_KEY)
    redis.call('DEL', SELECTED_USER_STATUS_KEY)
end

redis.register_function('connection_started', connection_started)
redis.register_function('connection_ended', connection_ended)
redis.register_function('set_user_status', set_user_status)
redis.register_function('clear_connections', clear_connections)
