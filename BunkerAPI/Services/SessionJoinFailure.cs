namespace BunkerAPI.Services;

public enum SessionJoinFailure
{
    None = 0,
    SessionNotFound = 1,
    InvalidName = 2,
    GameAlreadyStarted = 3,
    SessionFull = 4,
    DuplicateName = 5,
}
