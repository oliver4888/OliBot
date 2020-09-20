using Steam.Models.SteamStore;

namespace SteamHelper.Models
{
    public class SteamAppDetails
    {
        public SteamAppDetails() { }

        public static implicit operator SteamAppDetails(StoreAppDetailsDataModel model) =>
            new SteamAppDetails
            {
                SupportInfo = model.SupportInfo,
                ReleaseDate = model.ReleaseDate,
                Achievements = model.Achievements,
                Recommendations = model.Recommendations,
                Movies = model.Movies,
                Screenshots = model.Screenshots,
                Genres = model.Genres,
                Categories = model.Categories,
                Metacritic = model.Metacritic,
                Platforms = model.Platforms,
                PackageGroups = model.PackageGroups,
                Packages = model.Packages,
                PriceOverview = model.PriceOverview,
                Publishers = model.Publishers,
                Developers = model.Developers,
                //LinuxRequirements = model.LinuxRequirements,
                //MacRequirements = model.MacRequirements,
                Type = model.Type,
                Name = model.Name,
                SteamAppId = model.SteamAppId,
                RequiredAge = model.RequiredAge,
                ControllerSupport = model.ControllerSupport,
                IsFree = model.IsFree,
                Background = model.Background,
                Dlc = model.Dlc,
                AboutTheGame = model.AboutTheGame,
                ShortDescription = model.ShortDescription,
                SupportedLanguages = model.SupportedLanguages,
                HeaderImage = model.HeaderImage,
                Website = model.Website,
                //PcRequirements = model.PcRequirements,
                DetailedDescription = model.DetailedDescription,
                ContentDescriptors = model.ContentDescriptors
            };

        public StoreSupportInfoModel SupportInfo { get; set; }
        public StoreReleaseDateModel ReleaseDate { get; set; }
        public StoreAchievement Achievements { get; set; }
        public StoreRecommendationsModel Recommendations { get; set; }
        public StoreMovieModel[] Movies { get; set; }
        public StoreScreenshotModel[] Screenshots { get; set; }
        public StoreGenreModel[] Genres { get; set; }
        public StoreCategoryModel[] Categories { get; set; }
        public StoreMetacriticModel Metacritic { get; set; }
        public StorePlatformsModel Platforms { get; set; }
        public StorePackageGroupModel[] PackageGroups { get; set; }
        public string[] Packages { get; set; }
        public StorePriceOverview PriceOverview { get; set; }
        public string[] Publishers { get; set; }
        public string[] Developers { get; set; }
        //public dynamic LinuxRequirements { get; set; }
        //public dynamic MacRequirements { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public uint SteamAppId { get; set; }
        public uint RequiredAge { get; set; }
        public string ControllerSupport { get; set; }
        public bool IsFree { get; set; }
        public string Background { get; set; }
        public uint[] Dlc { get; set; }
        public string AboutTheGame { get; set; }
        public string ShortDescription { get; set; }
        public string SupportedLanguages { get; set; }
        public string HeaderImage { get; set; }
        public string Website { get; set; }
        //public dynamic PcRequirements { get; set; }
        public string DetailedDescription { get; set; }
        public StoreContentDescriptor ContentDescriptors { get; set; }
    }
}
