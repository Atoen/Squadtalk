#!lua name=typing

--!strict

local TYPING_USER_CHANNELS_KEY = 'user:typing:channels:'

local SHOULD_NOT_NOTIFY = 0
local SHOULD_NOTIFY = 1

local function user_is_typing(_, args)
    local channelId = args[1]
    local userId = args[2]

    local key = TYPING_USER_CHANNELS_KEY..userId

    local keyAdded = redis.call('SADD', key, channelId) == 1
    local ttl = redis.call('TTL', key)

    -- Also handles not ttl (-1)
    if ttl < 4 then
        redis.call('EXPIRE', key, 10)
    end

    local shouldNotifyRegardless = (ttl >= 0 and ttl < 4)

    if keyAdded or shouldNotifyRegardless then
        return SHOULD_NOTIFY
    else
        return SHOULD_NOT_NOTIFY
    end
end

-- No need to cleanup as the keys have relatively short TTL

redis.register_function('user_is_typing', user_is_typing)