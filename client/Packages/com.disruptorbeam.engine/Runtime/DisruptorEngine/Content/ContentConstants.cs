public static class ContentConstants
{
   public const string DATA_DIR = "Assets/DisruptorEngine/Editor/content";
   public const string ASSET_DIR = "Assets/DisruptorEngine/DefaultAssets";
   public const string DEFAULT_DATA_DIR = "Packages/com.disruptorbeam.engine/Editor/Content/DefaultContent";
   public const string DEFAULT_ASSET_DIR = "Packages/com.disruptorbeam.engine/Editor/Content/DefaultAssets~";

   //Websites
   public const string BEAMABLE_MAIN_WEBSITE = "beamable.com";
   public const string BEAMABLE_DOCS_WEBSITE = "docs.beamable.com";

   //Window Names
   public const string BEAMABLE = "Beamable";
   public const string CONTENT_MANAGER = "Content Manager";
   public const string THEME_MANAGER = "Theme Manager";
   public const string MICROSERVICES_MANAGER = "Microservices Manager";
   public const string PORTAL = "Portal";
   public const string TOOLBOX = "Toolbox";
   public const string BUSS = BEAMABLE + " Styles";
   public const string BUSS_SHEET_EDITOR = "Sheet Inspector";
   public const string BUSS_WIZARD = "Theme Wizard";

   //Help Urls
   public const string URL_BEAMABLE_MAIN_WEBSITE = "http://www.beamable.com";
   public const string URL_BEAMABLE_DOCS_WEBSITE = "http://docs.beamable.com";
   //
   public const string URL_FEATURE_ACCOUNT_HUD = "https://docs.beamable.com/docs/account-hud";
   public const string URL_FEATURE_ADMIN_FLOW = "https://docs.beamable.com/docs/admin-flow";
   public const string URL_FEATURE_ANNOUNCEMENTS_FLOW = "https://docs.beamable.com/docs/announcements-flow";
   public const string URL_FEATURE_CALENDAR_FLOW = "https://docs.beamable.com/docs/calendar-flow";
   public const string URL_FEATURE_CURRENCY_HUD = "https://docs.beamable.com/docs/currency-hud";
   public const string URL_FEATURE_LEADERBOARD_FLOW = "https://docs.beamable.com/docs/leaderboard-flow";
   public const string URL_FEATURE_LOGIN_FLOW = "https://docs.beamable.com/docs/login-flow";
   public const string URL_FEATURE_INVENTORY_FLOW = "https://docs.beamable.com/docs/inventory-flow";
   public const string URL_FEATURE_STORE_FLOW = "https://docs.beamable.com/docs/store-flow";

   //Menu Items: Shared
   public const string OPEN = "Open";

   //Menu Items: Window
   private const string MENU_ITEM_PATH_WINDOW = "Window";
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE = MENU_ITEM_PATH_WINDOW + "/Beamable";
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_SAMPLES = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Samples";
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Help";
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Utilities";
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER = MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Beamable Developer";
  public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_MICROSERVICES = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES + "/Microservices";

   //Menu Items: Window (#ifdef BEAMABLE_DEVELOPER)
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_BEAMABLE_DEVELOPER_SAMPLES = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/Samples";
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_UNITY = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/Unity";
   public const string MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_THEME_MANAGER = MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/Theme Manager";

   public const int MENU_ITEM_PATH_WINDOW_PRIORITY_1 = 0;
   public const int MENU_ITEM_PATH_WINDOW_PRIORITY_2 = 20;
   public const int MENU_ITEM_PATH_WINDOW_PRIORITY_3 = 40;
   public const int MENU_ITEM_PATH_WINDOW_PRIORITY_4 = 60;

   //Menu Items: Assets
   public const string MENU_ITEM_PATH_ASSETS_BEAMABLE = "Beamable";
   public const string MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS = MENU_ITEM_PATH_ASSETS_BEAMABLE + "/Configurations";
   public const string MENU_ITEM_PATH_ASSETS_BEAMABLE_SAMPLES = MENU_ITEM_PATH_ASSETS_BEAMABLE + "/Samples";
   public const int MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1 = 0;
}