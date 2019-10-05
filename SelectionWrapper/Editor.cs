﻿using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace SelectionWrapper
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EditorListener : IWpfTextViewCreationListener
    {
        private IWpfTextView TextView { get; set; }
        private IEditorOperations EditorOperations { get; set; }

        [Import]
        private IEditorOperationsFactoryService editorOperationsFactory = null;
        public Wrapper Wrapper { get; private set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            TextView = textView;
            TextView.GotAggregateFocus += OnTextViewFocus;
            TextView.TextBuffer.Changing += TextBuffer_Changing;
            TextView.TextBuffer.PostChanged += TextBuffer_PostChanged;

            EditorOperations = editorOperationsFactory.GetEditorOperations(TextView);
            Wrapper = new Wrapper(EditorOperations);
        }

        void OnTextViewFocus(object sender, EventArgs e)
        {
            var focusedTextView = sender as IWpfTextView;
            if (focusedTextView != null)
            {
                TextView = focusedTextView;
                EditorOperations = editorOperationsFactory.GetEditorOperations(TextView);
                Wrapper.EditorOperations = EditorOperations;
            }
        }

        private void TextBuffer_PostChanged(object sender, EventArgs e)
        {
            var textBuffer = sender as ITextBuffer;
            Wrapper.Wrap(textBuffer);
        }

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            Wrapper.CaptureSelectionState();
        }
    }
}
