namespace DataSeeder.Services;

public interface IPublicDataSeeder
{
    Task SeedDataAsync(int numberOfData);
    Task<bool> HasData();
}