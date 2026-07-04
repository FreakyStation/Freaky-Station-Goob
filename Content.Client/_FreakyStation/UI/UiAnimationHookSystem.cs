// SPDX-FileCopyrightText: 2026 Freak
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Content.Client.UserInterface.Controls;

namespace Content.Client._FreakyStation.UI;

/// <summary>
/// Scans the UI tree for <see cref="BaseButton"/> instances inside
/// <see cref="DefaultWindow"/> and <see cref="FancyWindow"/> and attaches
/// hover/click animation hooks from <see cref="UiAnimationSystem"/>.
/// </summary>
public sealed class UiAnimationHookSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    private readonly HashSet<Control> _scannedWindows = new();
    private readonly HashSet<BaseButton> _hookedButtons = new();
    private float _accumulator;

    private const float ScanInterval = 0.5f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        if (_accumulator < ScanInterval)
            return;
        _accumulator = 0f;

        ScanForNewButtons();
        CleanDisposed();
    }

    private static bool IsFullWindow(Control control)
    {
        return control is DefaultWindow or FancyWindow;
    }

    private void ScanForNewButtons()
    {
        foreach (var child in _ui.WindowRoot.Children)
        {
            if (!IsFullWindow(child))
                continue;

            if (!_scannedWindows.Add(child))
                continue;

            HookButtonsRecursive(child);
        }
    }

    private void HookButtonsRecursive(Control control)
    {
        if (control is BaseButton btn && _hookedButtons.Add(btn))
        {
            btn.OnMouseEntered += _ => UiAnimationSystem.AnimateHoverIn(btn);
            btn.OnMouseExited += _ => UiAnimationSystem.AnimateHoverOut(btn);
            btn.OnPressed += _ => UiAnimationSystem.AnimateClickPop(btn);
        }

        foreach (var child in control.Children)
        {
            HookButtonsRecursive(child);
        }
    }

    private void CleanDisposed()
    {
        _scannedWindows.RemoveWhere(c => c.Disposed);
        _hookedButtons.RemoveWhere(b => b.Disposed);
    }
}
