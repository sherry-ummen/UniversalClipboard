using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using UniveralClipboard;

namespace UniversalClipboard {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window {

        IntPtr _ClipboardViewerNext;
        IntPtr _handle; // Get initialized on Loaded event

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _handle = new WindowInteropHelper(this).Handle;
            RegisterClipboardViewer();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {

            switch ((UniveralClipboard.Msgs)msg) {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case UniveralClipboard.Msgs.WM_DRAWCLIPBOARD:

                    Debug.WriteLine("WindowProc DRAWCLIPBOARD: " + msg, "WndProc");

                    GetClipboardData();

                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    UniveralClipboard.User32.SendMessage(_ClipboardViewerNext, msg, wParam, lParam);
                    break;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case UniveralClipboard.Msgs.WM_CHANGECBCHAIN:
                    Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + lParam, "WndProc");

                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (wParam == _ClipboardViewerNext) {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        _ClipboardViewerNext = lParam;
                    } else {
                        UniveralClipboard.User32.SendMessage(_ClipboardViewerNext, msg, wParam, lParam);
                    }
                    break;

                default:
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //
                    handled = false;
                    break;

            }
            return hwnd;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            UnregisterClipboardViewer();
        }

        /// <summary>
		/// Register this form as a Clipboard Viewer application
		/// </summary>
		private void RegisterClipboardViewer() {
            _ClipboardViewerNext = User32.SetClipboardViewer(_handle);
        }

        /// <summary>
		/// Remove this form from the Clipboard Viewer list
		/// </summary>
		private void UnregisterClipboardViewer() {
            User32.ChangeClipboardChain(_handle, _ClipboardViewerNext);
        }

        private void GetClipboardData() {
            var data = Clipboard.GetDataObject();
            if (data.GetDataPresent("Text")) {
                ListBoxItem.Items.Add((string)data.GetData("Text"));
            }
        }
    }
}
