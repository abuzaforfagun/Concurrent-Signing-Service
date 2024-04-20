namespace DataSeeder.Services;

public interface IKeyStoreDataSeeder
{
    Task SeedDataAsync(int numberOfData);
    Task<bool> HasData();
}