using MICT.eDNA.Interfaces;

namespace MICT.eDNA.Managers
{
    public static class ServiceLocator
    {
        public static IStateService StateService { get; private set; }
        public static IExperienceService ExperienceService { get; private set; }
        public static IUserService UserService { get; private set; }
        public static IDataService DataService { get; private set; }
        public static INetworkService NetworkService { get; private set; }

        public static void AddService(IStateService stateService)
        {
            if (StateService == null)
            {
                StateService = stateService;
            }
        }

        public static void AddService(IExperienceService experienceService)
        {
            if (ExperienceService == null)
            {
                ExperienceService = experienceService;
            }
        }

        public static void AddService(IUserService userService)
        {
            if (UserService == null)
            {
                UserService = userService;
            }
        }

        public static void AddService(IDataService dataService)
        {
            if (DataService == null)
            {
                DataService = dataService;
            }
        }

        public static void ReplaceService(IDataService dataService)
        {
            if (DataService != null) {
                DataService.CloseStream();
            }
            DataService = dataService;
        }

        public static void AddService(INetworkService networkService)
        {
            if (NetworkService == null)
            {
                NetworkService = networkService;
            }
        }
    }

}