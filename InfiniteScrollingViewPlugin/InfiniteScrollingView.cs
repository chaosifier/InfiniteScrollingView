using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using System.Diagnostics;

namespace InfiniteScrollingViewPlugin
{
    public class InfiniteScrollingView<SourceItemType> : ContentView
    {
        public int TotalColumns { get; set; } = 1;
        public ObservableCollection<SourceItemType> SourceItems { get; set; } = new ObservableCollection<SourceItemType>();
        public Action<InfiniteScrollingView<SourceItemType>> LoadMoreAction { get; set; }
        public int IndividualItemHeight { get; set; } = 20;
        public int ScrollPercentToLoadMore { get; set; } = 90;
        public double ColumnsSpacing { get; set; } = 20;
        public DataTemplate ItemDataTemplate { get; set; }
        public bool FirstBatchLoaded = false;

        private ScrollView _mainContainerSv;
        private StackLayout _mainContainerSl;
        private List<ListView> _listViews = new List<ListView>();
        private List<ObservableCollection<SourceItemType>> _listViewItemSources = new List<ObservableCollection<SourceItemType>>();


        /**
         * NOTE : For OuterScroll to work, the sum of heights of each listview's row must be less than or equals to the height of the container.
         * For this, set the RowHeight to some value and HasUnevenRows to false and later calculate the actual height of the ListView
         * */

        public InfiniteScrollingView()
        {
            _mainContainerSl = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                //Spacing = ColumnsSpacing,
                BackgroundColor = Color.LightGreen
            };

            _mainContainerSv = new ScrollView()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Content = _mainContainerSl,
                BackgroundColor = Color.LightSalmon
            };

            _mainContainerSv.Scrolled += _mainContainerSv_Scrolled;

            Content = _mainContainerSv;
        }

        private bool _isBusy = false;
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            _mainContainerSv.WidthRequest = width;
            _mainContainerSv.HeightRequest = height;

            //if (height > _mainContainerSv.HeightRequest)
            //{
            //    //LoadMore();
            //}
            ScrollOrSizeChangeHandler(true);

            //ReorganizeListView();
        }

        private void _mainContainerSv_Scrolled(object sender, ScrolledEventArgs e)
        {
            ScrollOrSizeChangeHandler();
        }

        private void ScrollOrSizeChangeHandler(bool reorganize = false)
        {
            bool reorganized = false;
            if (_mainContainerSv.ScrollY >= 0 && this.Height > 0)
            {
                var totalDisplayedScrollableSection = _mainContainerSv.ScrollY + this.Height;
                var totalDisplayedScrollableSectionPercent = totalDisplayedScrollableSection / _mainContainerSl.HeightRequest * 100;
                //var scrolledPercent = _mainContainerSv.ScrollY / _mainContainerSv.HeightRequest * 200;

                Debug.WriteLine(totalDisplayedScrollableSectionPercent);
                if (totalDisplayedScrollableSectionPercent >= ScrollPercentToLoadMore)
                {
                    LoadMore();
                    reorganized = true;
                }
            }

            if (reorganize && !reorganized)
            {
                double widthPerColumn = (_mainContainerSv.WidthRequest - (_mainContainerSl.Padding.Left + _mainContainerSl.Padding.Right + _mainContainerSl.Spacing * (TotalColumns <= 1 ? 1 : TotalColumns - 1))) / TotalColumns;
                Debug.WriteLine("new width per column : " + widthPerColumn);
                for (int i = 0; i < _listViews.Count; i++)
                {
                    _listViews[i].WidthRequest = widthPerColumn;
                }
            }
        }

        /// <summary>
        /// Reorganizes views depending on SourceItems and TotalColumns properties
        /// </summary>
        /// <returns></returns>
        private void ReorganizeListView()
        {
            bool mappingNeeded = TotalColumns != _listViews.Count;

            int itemsPerColumn = SourceItems.Count < TotalColumns ? SourceItems.Count : SourceItems.Count / TotalColumns;
            int additionalItemsInFirstColumn = SourceItems.Count < TotalColumns ? 0 : SourceItems.Count % TotalColumns;

            double maxHeight = (itemsPerColumn + additionalItemsInFirstColumn) * IndividualItemHeight;

            double widthPerColumn = (_mainContainerSv.WidthRequest - (_mainContainerSl.Padding.Left + _mainContainerSl.Padding.Right + _mainContainerSl.Spacing * (TotalColumns <= 1 ? 1 : TotalColumns - 1))) / TotalColumns;

            _mainContainerSl.Orientation = TotalColumns > 1 ? StackOrientation.Horizontal : StackOrientation.Vertical;

            //_mainContainerSv.HeightRequest = maxHeight;
            _mainContainerSl.HeightRequest = maxHeight;

            int takenCounter = 0;
            for (int i = 0; i < TotalColumns; i++)
            {
                if (_listViewItemSources.Count < i + 1)
                {
                    _listViewItemSources.Add(new ObservableCollection<SourceItemType>());
                }
                int itemsCountToTake = itemsPerColumn + (i == 0 ? additionalItemsInFirstColumn : 0);
                var itemsToTake = SourceItems.Skip(takenCounter).Take(itemsCountToTake);

                _listViewItemSources[i].Clear();
                foreach (var item in itemsToTake)
                {
                    _listViewItemSources[i].Add(item);
                }

                if (_listViews.Count < i + 1 || _listViews[i] == null)
                {
                    _listViews.Add(new ListView(ListViewCachingStrategy.RecycleElement)
                    {
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.Fill,
                        WidthRequest = widthPerColumn,
                        HeightRequest = IndividualItemHeight * _listViewItemSources[i].Count,
                        BackgroundColor = Color.LightYellow,
                        RowHeight = IndividualItemHeight
                    });
                }


                //if (takenCounter + itemsToTake > SourceItems.Count)
                //{
                //    break;
                //}



                takenCounter += itemsCountToTake;

                _listViews[i].ItemTemplate = ItemDataTemplate;

                _listViews[i].ItemsSource = _listViewItemSources[i];
            }

            //if (mappingNeeded)
            //{
            MapListViewCollectionToSLChildren();
            //}
        }

        private void MapListViewCollectionToSLChildren()
        {
            _mainContainerSl.Children.Clear();
            _listViews.ToList().ForEach(v => _mainContainerSl.Children.Add(v));
        }

        /// <summary>
        /// Loads data to the SourceItems collection
        /// </summary>
        /// <returns></returns>
        public void LoadMore()
        {
            if (!_isBusy)
            {
                _isBusy = true;
                Task.Run(() => LoadMoreAction.Invoke(this)).Wait();
                ReorganizeListView();
                _isBusy = false;
            }
        }
    }
}