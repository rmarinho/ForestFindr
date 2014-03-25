using System.ComponentModel.Composition;
using System.Windows.Controls;
using ForestFindr.ViewModels;

namespace ForestFindr.Views
{
    public partial class PhotosView : UserControl
    {
        #region Ctor
		
        public PhotosView()
        {
            InitializeComponent();
            CompositionInitializer.SatisfyImports(this);
        } 
	 #endregion

        #region View Model
        [Import]
        public PhotosViewModel ViewModel
        {
            get
            {

                return (DataContext as PhotosViewModel);

            }
            set
            {
                DataContext = value;

            }
        }
        
        #endregion
      
    }
}
