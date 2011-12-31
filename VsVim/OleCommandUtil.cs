﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Vim;
using Vim.Extensions;

namespace VsVim
{
    internal static class OleCommandUtil
    {
        internal static bool TryConvert(Guid commandGroup, uint commandId, IntPtr pVariableIn, KeyModifiers modifiers, out EditCommand command)
        {
            KeyInput keyInput;
            EditCommandKind kind;
            if (!TryConvert(commandGroup, commandId, pVariableIn, out keyInput, out kind))
            {
                command = null;
                return false;
            }

            keyInput = KeyInputUtil.ApplyModifiers(keyInput, modifiers);
            command = new EditCommand(keyInput, kind, commandGroup, commandId);
            return true;
        }

        internal static bool TryConvert(Guid commandGroup, OleCommandData oleCommandData, out KeyInput keyInput)
        {
            EditCommandKind editCommandKind;
            return TryConvert(commandGroup, oleCommandData.CommandId, oleCommandData.VariantIn, out keyInput, out editCommandKind);
        }

        internal static bool TryConvert(Guid commandGroup, uint commandId, IntPtr variantIn, out KeyInput keyInput, out EditCommandKind kind)
        {
            if (VSConstants.GUID_VSStandardCommandSet97 == commandGroup)
            {
                return TryConvert((VSConstants.VSStd97CmdID)commandId, variantIn, out keyInput, out kind);
            }

            if (VSConstants.VSStd2K == commandGroup)
            {
                return TryConvert((VSConstants.VSStd2KCmdID)commandId, variantIn, out keyInput, out kind);
            }

            keyInput = null;
            kind = EditCommandKind.UserInput;
            return false;
        }

        /// <summary>
        /// Try and convert a Visual Studio 2000 style command into the associated KeyInput and EditCommand items
        /// </summary>
        internal static bool TryConvert(VSConstants.VSStd2KCmdID cmdId, IntPtr variantIn, out KeyInput keyInput, out EditCommandKind kind)
        {
            switch (cmdId)
            {
                case VSConstants.VSStd2KCmdID.TYPECHAR:
                    if (variantIn == IntPtr.Zero)
                    {
                        keyInput = KeyInputUtil.CharToKeyInput(Char.MinValue);
                    }
                    else
                    {
                        var obj = Marshal.GetObjectForNativeVariant(variantIn);
                        var c = (char)(ushort)obj;
                        keyInput = KeyInputUtil.CharToKeyInput(c);
                    }
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.RETURN:
                    keyInput = KeyInputUtil.EnterKey;
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.CANCEL:
                    keyInput = KeyInputUtil.EscapeKey;
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.DELETE:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Delete);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.BACKSPACE:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Back);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.LEFT:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Left);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.LEFT_EXT:
                case VSConstants.VSStd2KCmdID.LEFT_EXT_COL:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.Left), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.RIGHT:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Right);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.RIGHT_EXT:
                case VSConstants.VSStd2KCmdID.RIGHT_EXT_COL:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.Right), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.UP:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Up);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.UP_EXT:
                case VSConstants.VSStd2KCmdID.UP_EXT_COL:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.Up), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.DOWN:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Down);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.DOWN_EXT:
                case VSConstants.VSStd2KCmdID.DOWN_EXT_COL:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.Down), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.TAB:
                    keyInput = KeyInputUtil.TabKey;
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.BACKTAB:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.TabKey, KeyModifiers.Shift);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.PAGEDN:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.PageDown);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.PAGEDN_EXT:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.PageDown), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.PAGEUP:
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.PageUp);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.PAGEUP_EXT:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.PageUp), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.UNDO:
                case VSConstants.VSStd2KCmdID.UNDONOMOVE:
                    // Visual Studio was asked to undo.  This happens when either the undo button
                    // was hit or the visual studio key combination bound to the undo command 
                    // was executed
                    keyInput = KeyInput.DefaultValue;
                    kind = EditCommandKind.Undo;
                    break;
                case VSConstants.VSStd2KCmdID.REDO:
                case VSConstants.VSStd2KCmdID.REDONOMOVE:
                    // Visual Studio was asked to redo.  This happens when either the redo button
                    // was hit or the visual studio key combination bound to the redo command 
                    // was executed
                    keyInput = KeyInput.DefaultValue;
                    kind = EditCommandKind.Redo;
                    break;
                case VSConstants.VSStd2KCmdID.BOL:
                    // Even though there as a HOME value defined, Visual Studio apparently maps the 
                    // Home key to BOL
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Home);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.BOL_EXT:
                case VSConstants.VSStd2KCmdID.BOL_EXT_COL:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.Home), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.EOL:
                    // Even though there as a END value defined, Visual Studio apparently maps the 
                    // Home key to EOL
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.End);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd2KCmdID.EOL_EXT:
                case VSConstants.VSStd2KCmdID.EOL_EXT_COL:
                    keyInput = KeyInputUtil.ApplyModifiers(KeyInputUtil.VimKeyToKeyInput(VimKey.End), KeyModifiers.Shift);
                    kind = EditCommandKind.VisualStudioCommand;
                    break;
                case VSConstants.VSStd2KCmdID.TOGGLE_OVERTYPE_MODE:
                    // The <Insert> key is expressed in the toggle overtype mode flag.  In general
                    // over write mode is referred to as overtype in the code / documentation
                    keyInput = KeyInputUtil.VimKeyToKeyInput(VimKey.Insert);
                    kind = EditCommandKind.UserInput;
                    break;
                default:
                    keyInput = null;
                    kind = EditCommandKind.UserInput;
                    break;
            }

            return keyInput != null;
        }

        /// <summary>
        /// Try and convert the Visual Studio 97 based command into KeyInput and EditCommandKind values
        /// </summary>
        internal static bool TryConvert(VSConstants.VSStd97CmdID cmdId, IntPtr variantIn, out KeyInput ki, out EditCommandKind kind)
        {
            ki = null;
            kind = EditCommandKind.UserInput;

            switch (cmdId)
            {
                case VSConstants.VSStd97CmdID.SingleChar:
                    var obj = Marshal.GetObjectForNativeVariant(variantIn);
                    var c = (char)(ushort)obj;
                    ki = KeyInputUtil.CharToKeyInput(c);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd97CmdID.Escape:
                    ki = KeyInputUtil.EscapeKey;
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd97CmdID.Delete:
                    ki = KeyInputUtil.VimKeyToKeyInput(VimKey.Delete);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd97CmdID.F1Help:
                    ki = KeyInputUtil.VimKeyToKeyInput(VimKey.F1);
                    kind = EditCommandKind.UserInput;
                    break;
                case VSConstants.VSStd97CmdID.Undo:
                    ki = KeyInput.DefaultValue;
                    kind = EditCommandKind.Undo;
                    break;
                case VSConstants.VSStd97CmdID.Redo:
                    ki = KeyInput.DefaultValue;
                    kind = EditCommandKind.Redo;
                    break;
                case VSConstants.VSStd97CmdID.MultiLevelUndo:
                    // This occurs when the undo button is pressed.  If it's just simply pressed we get 
                    // a IntPtr.Zero 'variantIn' value and can proceed with Vim undo.  Else user selected
                    // a very specific undo point and we shouldn't mess with it
                    if (variantIn == IntPtr.Zero)
                    {
                        ki = KeyInput.DefaultValue;
                        kind = EditCommandKind.Undo;
                    }
                    break;
                case VSConstants.VSStd97CmdID.MultiLevelRedo:
                    // This occurs when the redo button is pressed.  If it's just simply pressed we get 
                    // a IntPtr.Zero 'variantIn' value and can proceed with Vim redo .  Else user selected
                    // a very specific redo point and we shouldn't mess with it
                    if (variantIn == IntPtr.Zero)
                    {
                        ki = KeyInput.DefaultValue;
                        kind = EditCommandKind.Redo;
                    }
                    break;
            }

            return ki != null;
        }

        /// <summary>
        /// Try and convert the KeyInput value into an OleCommandData instance
        /// </summary>
        internal static bool TryConvert(KeyInput keyInput, out Guid commandGroup, out OleCommandData oleCommandData)
        {
            var success = true;
            commandGroup = VSConstants.VSStd2K;
            switch (keyInput.Key)
            {
                case VimKey.Enter:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.RETURN);
                    break;
                case VimKey.Escape:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.CANCEL);
                    break;
                case VimKey.Delete:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.DELETE);
                    break;
                case VimKey.Back:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.BACKSPACE);
                    break;
                case VimKey.Up:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.UP);
                    break;
                case VimKey.Down:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.DOWN);
                    break;
                case VimKey.Left:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.LEFT);
                    break;
                case VimKey.Right:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.RIGHT);
                    break;
                case VimKey.Tab:
                    oleCommandData = new OleCommandData(keyInput.KeyModifiers == KeyModifiers.Shift ? VSConstants.VSStd2KCmdID.BACKTAB : VSConstants.VSStd2KCmdID.TAB);
                    break;
                case VimKey.PageUp:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.PAGEUP);
                    break;
                case VimKey.PageDown:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.PAGEDN);
                    break;
                case VimKey.Insert:
                    oleCommandData = new OleCommandData(VSConstants.VSStd2KCmdID.TOGGLE_OVERTYPE_MODE);
                    break;
                default:
                    if (keyInput.RawChar.IsSome())
                    {
                        oleCommandData = OleCommandData.Allocate(keyInput.Char);
                    }
                    else
                    {
                        oleCommandData = new OleCommandData();
                        success = false;
                    }
                    break;
            }

            return success;
        }
    }
}
