﻿using Squadtalk.Shared;

namespace Squadtalk.Server.Services;

public interface IHashService
{
    int HashBytes { get; set; }

    Task<string> HashAsync(UserCredentialsDto credentialsDto, byte[] salt);

    byte[] GetSalt(int count);
}