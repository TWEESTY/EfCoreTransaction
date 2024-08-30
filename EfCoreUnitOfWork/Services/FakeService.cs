
namespace EfCoreUnitOfWork.Services
{
    public class FakeService : IFakeService
    {
        public Task DoWorkAsync()
        {
            return Task.CompletedTask;
        }
    }
}
