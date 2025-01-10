#!lua name=user_connections

--!strict

local CONNECTION_KEY = 'user:connections:'
local USER_STATUS_KEY = 'user:status'

-- enum UserStatus
-- {
--     Unknown = 0,
--     Online = 1,
--     Away = 2,
--     DoNotDisturb = 3,
--     Offline = 4
-- }

-- If user has set custom status, reuse it
-- Online and Away (caused by inactivity only) are overriden by default
local function connection_started(_, args)
    local userId = args[1]
    local connectionId = args[2]

    redis.call('SADD', CONNECTION_KEY..userId, connectionId)

    local statusChanged = 0
    local currentStatus = tonumber(redis.call('HGET', USER_STATUS_KEY, userId) or -1)

    if currentStatus == -1 then
        statusChanged = 1
        currentStatus = 1
        redis.call('HSET', USER_STATUS_KEY, userId, currentStatus)
    end

    return {statusChanged, currentStatus}
end

local function connection_ended(_, args)
    local userId = args[1]
    local connectionId = args[2]

    local userConnectionsKey = CONNECTION_KEY..userId
    redis.call('SREM', userConnectionsKey, connectionId)
    local remainingConnectionCount = redis.call('SCARD', userConnectionsKey)

    local currentStatus = tonumber(redis.call('HGET', USER_STATUS_KEY, userId) or -1)

    -- Other connections exist or user set their status as 'Offline'?
    if remainingConnectionCount > 0 or currentStatus == 4 then
        -- No visible change in status
        return {0, currentStatus}
    end

    -- Preserve manually set status that is not 'Online'
    -- If status if 'Online', remove it and return 'Offline'
    if currentStatus == 1 then
        redis.call('HDEL', USER_STATUS_KEY, userId)
    end

    return {1, 4} -- true, Offline
end

local function set_user_status(_, args)
    local userId = args[1]
    local newStatus = tonumber(args[2])

    local currentStatus = tonumber(redis.call('HGET', USER_STATUS_KEY, userId) or -1)
    if (currentStatus == newStatus) then
        return {0, currentStatus}
    end

    redis.call('HSET', USER_STATUS_KEY, userId, newStatus)

    return {1, newStatus}
end

local function clear_connections(_, _)
    local connectionKeys = redis.call('KEYS', CONNECTION_KEY .. '*')

    for _, key in ipairs(connectionKeys) do
        redis.call('DEL', key)
    end

    redis.call('DEL', USER_STATUS_KEY)
end

redis.register_function('connection_started', connection_started)
redis.register_function('connection_ended', connection_ended)
redis.register_function('set_user_status', set_user_status)
redis.register_function('clear_connections', clear_connections)
