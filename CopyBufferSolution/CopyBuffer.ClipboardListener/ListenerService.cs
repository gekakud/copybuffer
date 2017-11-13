using System;
using System.Windows.Forms;
using System.Diagnostics;
using RAD.ClipMon.Win32;
using System.Collections.Concurrent;
using CopyBuffer.Service.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CopyBuffer.Core;

namespace CopyBuffer.ClipboardListener
{
    public class ListenerService : Form ,ICopyBufferService, IDisposable
    {
        #region Data Members

        private readonly ConcurrentBag<BufferItem> _copyBuffer;

        private IntPtr _clipboardViewerNext;

        #endregion

        #region Singleton Instance

        private static ListenerService _instance;

        public static ListenerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ListenerService();
                    return _instance;
                }
                return _instance;
            }

        }

        #endregion

        #region CTOR

        public ListenerService()
        {
            _copyBuffer = new ConcurrentBag<BufferItem>();
        }

        #endregion

        #region Events Un/Registration

        /// <summary>
        /// Register this form as a Clipboard Viewer application
        /// </summary>
        private void RegisterClipboardViewer()
        {
            _clipboardViewerNext = User32.SetClipboardViewer(this.Handle);
        }

        /// <summary>
        /// Remove this form from the Clipboard Viewer list
        /// </summary>
        private void UnregisterClipboardViewer()
        {
            User32.ChangeClipboardChain(this.Handle, _clipboardViewerNext);
        }

        #endregion

        #region Event Handlers

        protected override void WndProc(ref Message m)
        {
            switch ((Msgs) m.Msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case Msgs.WM_DRAWCLIPBOARD:

                    Debug.WriteLine("WindowProc DRAWCLIPBOARD: " + m.Msg, "WndProc");

                    GetClipboardData();

                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    User32.SendMessage(_clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    break;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case Msgs.WM_CHANGECBCHAIN:
                    Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + m.LParam, "WndProc");

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
                    if (m.WParam == _clipboardViewerNext)
                    {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        _clipboardViewerNext = m.LParam;
                    }
                    else
                    {
                        User32.SendMessage(_clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    }
                    break;

                default:
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //
                    base.WndProc(ref m);
                    break;

            }

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Show the clipboard contents in the window 
        /// and show the notification balloon if a link is found
        /// </summary>
        private void GetClipboardData()
        {
            //
            // Data on the clipboard uses the 
            // IDataObject interface
            //
            IDataObject iData = new DataObject();

            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (ExternalException externEx)
            {
                // Copying a field definition in Access 2002 causes this sometimes?
                Debug.WriteLine("InteropServices.ExternalException: {0}", externEx.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            if (iData.GetDataPresent(DataFormats.Text))
            {
                var itemStr = (string) iData.GetData(DataFormats.Text);
                var item = new BufferItem
                {
                    TextContent = itemStr,
                    TimeStamp = DateTime.Now,
                    ItemType = itemStr == null ? BufferItemType.Image : BufferItemType.Text
                };

                if (item.ItemType == BufferItemType.Text && item.TextContent != null)
                {
                    _copyBuffer.Add(item);
                }

                Debug.WriteLine((string) iData.GetData(DataFormats.Text));
            }
        }

        #endregion

        #region ICopyBufferService

        public void Start()
        {
            RegisterClipboardViewer();
        }

        public void Stop()
        {
            UnregisterClipboardViewer();
        }

        public void ClearBufferHistory()
        {
            BufferItem someItem;
            while (!_copyBuffer.IsEmpty)
            {
                _copyBuffer.TryTake(out someItem);
            }
        }

        public List<BufferItem> GetBufferedHistory()
        {
            return _copyBuffer.ToList();
        }

        public void SetItemToClipboard(BufferItem p_item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public new void Dispose()
        {
            UnregisterClipboardViewer();
        }

        #endregion

    }

}
