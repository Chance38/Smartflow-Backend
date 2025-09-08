namespace SmartFlowBackend.Application.SwaggerSetting;

public static class TestUser
{
    // This is a fixed test UserId for easy testing.
    public static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string Username = "Chance";
    public const string Account = "chance";
    public const string Password = "password";
    public const float InitialBalance = 1000.0f;
}
