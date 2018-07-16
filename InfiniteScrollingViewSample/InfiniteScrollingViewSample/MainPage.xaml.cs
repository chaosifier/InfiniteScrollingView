using InfiniteScrollingViewPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace InfiniteScrollingViewSample
{
    public partial class MainPage : ContentPage
    {
        private int _currentPage = 0;
        private int _pageSize = 50;
        private InfiniteScrollingView<Comment> _infScrView;
        private int _tempCounter = 0;

        public MainPage()
        {
            InitializeComponent();

            _infScrView = new InfiniteScrollingView<Comment>()
            {
                BackgroundColor = Color.LightBlue,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                IndividualItemHeight = 40,
                TotalColumns = 2
            };

            _infScrView.ItemDataTemplate = new DataTemplate(() =>
            {
                var tc = new TextCell();

                tc.SetBinding(TextCell.TextProperty, nameof(Comment.name));
                tc.SetBinding(TextCell.DetailProperty, nameof(Comment.body));

                return tc;
            });

            _infScrView.LoadMoreAction = ((o) =>
            {
                //var apiReqTask = Task.Run(async () => await ApiRequestHandler.MakeApiRequestForListAsync<Comment>("https://my-json-server.typicode.com/chaosifier/FakeREST/FormattedComments"));
                //apiReqTask.Wait();
                //var apiResp = apiReqTask.Result;

                //if (apiResp.Success)
                //{
                //    var itemsInThisPage = apiResp.Result.Skip(_currentPage * _pageSize).Take(_pageSize);
                //    _currentPage++;

                //    foreach (var item in itemsInThisPage)
                //    {
                //        _infScrView.SourceItems.Add(item);
                //    }
                //}

                for (int i = 0; i < _pageSize; i++)
                {
                    _tempCounter++;
                    _infScrView.SourceItems.Add(new Comment()
                    {
                        body = $"body {_tempCounter}",
                        name = $"name {_tempCounter}"
                    });
                }
            });

            Content = _infScrView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!_infScrView.FirstBatchLoaded)
            {
                _infScrView.LoadMore();
                _infScrView.FirstBatchLoaded = true;
            }
        }
    }

    public class Comment
    {
        public int postId { get; set; }
        public int Id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string body { get; set; }
    }
}
