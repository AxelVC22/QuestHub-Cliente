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

        public User User { get; set; }

        public IPostsService PostsService { get; set; }

        public IAnswersService AnswersService { get; set; }

        public IRatingsService RatingsService { get; set; }

        public ICategoriesService CategoriesService { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            User = new User();

            AuthService = new AuthService(new HttpClient());

            PostsService = new PostsService(new HttpClient());

            AnswersService = new AnswersService(new HttpClient());

            RatingsService = new RatingsService(new HttpClient());

            CategoriesService = new CategoriesService(new HttpClient());

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
                         NavigationService, AuthService);
                     return new Views.CreateUserView(viewModel);
                 }
                 else if (viewModelType == typeof(HomeViewModel))
                 {

                     var viewModel = new HomeViewModel(
                         NavigationService, PostsService);
                     return new Views.HomeView(viewModel);
                 }
                 else if (viewModelType == typeof(PostViewModel))
                 {
                     var post = parameter as Post;
                     var viewModel = new PostViewModel(
                         NavigationService, post, AnswersService, RatingsService);
                     return new Views.PostView(viewModel);
                 }
                 else if (viewModelType == typeof(NewPostViewModel))
                 {

                     var viewModel = new NewPostViewModel(
                         NavigationService, CategoriesService, PostsService);
                     return new Views.NewPostView(viewModel);
                 }
                 else if (viewModelType == typeof(ProfileViewModel))
                 {
                     var viewModel = new ProfileViewModel(
                         NavigationService);
                     return new Views.ProfileView(viewModel);
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
    }
}