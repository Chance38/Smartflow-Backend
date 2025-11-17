namespace Domain.Interface;

public interface IUserRegisterSubscriber
{
    Task HandleAsync(string message);
}
