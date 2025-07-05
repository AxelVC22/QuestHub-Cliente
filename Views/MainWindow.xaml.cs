using Multimedia;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.ViewModels;
using QuestHubClient.Views;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuestHubClient
{

    public partial class MainWindow : Window
    {
        public INavigationService NavigationService { get; set; }

        public IAuthService AuthService { get; set; }
        public IUserService UserService { get; set; }
        HttpClient HttpClient { get; set; }
        public User User { get; set; }

        public IPostsService PostsService { get; set; }

        public IAnswersService AnswersService { get; set; }

        public IRatingsService RatingsService { get; set; }

        public ICategoriesService CategoriesService { get; set; }

        public IFollowingService FollowingService { get; set; }

        public IReportsService ReportsService { get; set; }

        public MultimediaUploadService MultimediaService;

        public MainWindow()
        {
            InitializeComponent();

            User = new User();
            HttpClient = new HttpClient();

            AuthService = new AuthService(HttpClient);
            UserService = new UserService(HttpClient);
            FollowingService = new FollowingService(HttpClient);


            PostsService = new PostsService(new HttpClient());

            AnswersService = new AnswersService(new HttpClient());

            RatingsService = new RatingsService(new HttpClient());

            CategoriesService = new CategoriesService(new HttpClient());

            ReportsService = new ReportsService(HttpClient);
            InitializeMultimediaService();

            NavigationService = new FrameNavigationService(
             PageFrame,
             (viewModelType, parameter) =>
             {
                 if (viewModelType == typeof(LoginViewModel))
                 {
                     var viewModel = new LoginViewModel(
                        NavigationService, AuthService, new LoginUser()
                     );

                     return new Views.LoginView(viewModel);
                 }
                 else if (viewModelType == typeof(CreateUserViewModel))
                 {

                     var viewModel = new CreateUserViewModel(
                         NavigationService, AuthService, new User());
                     return new Views.CreateUserView(viewModel);
                 }
                 else if (viewModelType == typeof(HomeViewModel))
                 {


                     var viewModel = new HomeViewModel(
                         NavigationService, PostsService, AnswersService, FollowingService, MultimediaService, UserService, CategoriesService);
                     return new Views.HomeView(viewModel);
                 }
                 else if (viewModelType == typeof(PostViewModel))
                 {
                     var post = parameter as Post;
                     var viewModel = new PostViewModel(
                         NavigationService, post, AnswersService, RatingsService, PostsService, FollowingService, MultimediaService, UserService);
                     return new Views.PostView(viewModel);
                 }
                 else if (viewModelType == typeof(NewPostViewModel))
                 {
                     var post = parameter as Post ?? new Post();

                     var viewModel = new NewPostViewModel(post,
                         NavigationService, CategoriesService, PostsService, MultimediaService);
                     return new Views.NewPostView(viewModel);
                 }
                 else if (viewModelType == typeof(ProfileViewModel))
                 {

                     var user = parameter as User ?? App.MainViewModel.User;

                     var viewModel = new ProfileViewModel(NavigationService, UserService, user);
                     return new Views.ProfileView(viewModel);
                 }
                 else if (viewModelType == typeof(CategoriesViewModel))
                 {
                     var viewModel = new CategoriesViewModel(
                         NavigationService, CategoriesService);
                     return new Views.CategoriesView(viewModel);

                 }
                 else if (viewModelType == typeof(UsersViewModel))
                 {
                     var viewModel = new UsersViewModel(
                         NavigationService, UserService);
                     return new Views.UsersView(viewModel);

                 }
                 else if (viewModelType == typeof(NewReportViewModel))
                 {
                     var report = parameter as Report;

                     var viewModel = new NewReportViewModel(report, NavigationService, PostsService, AnswersService, FollowingService,ReportsService, MultimediaService, UserService
                         );
                     return new Views.NewReportView(viewModel);

                 }
                 else if (viewModelType == typeof(ReportsViewModel))
                 {

                     var viewModel = new ReportsViewModel(ReportsService, NavigationService, 
                         AnswersService, PostsService, RatingsService, FollowingService, MultimediaService, UserService
                         );
                     return new Views.ReportsView(viewModel);

                 }
                 else if (viewModelType == typeof(StatisticsViewModel))
                 {

                     var viewModel = new StatisticsViewModel(UserService
                         );
                     return new Views.StatisticsView(viewModel);

                 }

                 //else if (viewModelType == typeof(HomeViewModel))
                 //{

                 //    var viewModel = new HomeViewModel(
                 //        NavigationService);
                 //    return new View.Pages.Home(viewModel);
                 //}
                 //else if (viewModelType == typeof(ProfileViewModel))
                 //{

                 //    var viewModel = new ProfileViewModel(
                 //        NavigationService);
                 //    return new View.Pages.Profile(viewModel);
                 //}
                 //else if (viewModelType == typeof(NewPostViewModel))
                 //{

                 //    var viewModel = new NewPostViewModel(
                 //        NavigationService);
                 //    return new View.Pages.NewPost(viewModel);
                 //}
                 throw new ArgumentException("ViewModel desconocido");
             });

            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(NavigationService);
            this.DataContext = mainWindowViewModel;
            App.MainViewModel = mainWindowViewModel;

            NavigationService.NavigateTo<HomeViewModel>();


        }
        private void InitializeMultimediaService()
        {
            try
            {
                var grpcServerUrl = "http://10.48.138.135:50052";
                MultimediaService = new MultimediaUploadService(grpcServerUrl);
            }
            catch (Exception ex)
            {
                new NotificationWindow(ex.Message, 3).Show();
            }
        }
    }
}