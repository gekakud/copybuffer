using System;
using System.Windows.Forms;
using FMUtils.KeyboardHook;
using Hook = FMUtils.KeyboardHook.Hook;

namespace CopyBuffer.Ui.Wpf.Common
{
    internal class KeyEventHandler
    {
        private readonly Action _fireOnKeyMatch;
        private readonly Hook _keyboardHook;

        public KeyEventHandler(Action p_fireOnMatch)
        {
            _fireOnKeyMatch = p_fireOnMatch;

            _keyboardHook = new Hook("Global Action Hook");
            _keyboardHook.KeyDownEvent -= KeyDown;
            _keyboardHook.KeyDownEvent += KeyDown;
        }

        private void KeyDown(KeyboardHookEventArgs e)
        {
            if (e.Key == Keys.Q && e.isCtrlPressed)
            {
                _fireOnKeyMatch?.Invoke();
            }
        }

        public void UnsubscribeFromEvent()
        {
            _keyboardHook.KeyDownEvent -= KeyDown;
        }
    }
}