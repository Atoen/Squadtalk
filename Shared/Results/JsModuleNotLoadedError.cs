using FluentResults;

namespace Shared.Results;

public class JsModuleNotLoadedError(string identifier)
    : Error($"Unable to call {identifier} because the module is not loaded");

public class JsInvocationError(string identifier, Exception exception)
    : Error($"An error occured when invoking {identifier}: {exception.Message}");