using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Data.TypedIds;
using Squadtalk.Data.TypedIds;

namespace Squadtalk.Data;

public class UserIdConverter() : ValueConverter<UserId, Guid>(x => x.Value, x => new UserId(x));

public class GroupIdConverter() : ValueConverter<GroupId, string>(x => x.Value, x => new GroupId(x));

public class FriendRequestIdConverter() : ValueConverter<FriendRequestId, int>(x => x.Value, x => new FriendRequestId(x));

public class TusFileIdConverter() : ValueConverter<TusFileId, string>(x => x.Value, x => new TusFileId(x));

public class MessageIdConverter() : ValueConverter<MessageId, uint>(x => x.Value, x => new MessageId(x));
