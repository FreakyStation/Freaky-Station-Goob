// SPDX-FileCopyrightText: 2026 Freak
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._FreakyStation.UI;

/// <summary>
/// Applies entrance animations to UI controls.
/// Interpolates <see cref="Control.ModulateSelf"/> alpha each frame for a smooth fade-in.
/// Detects newly opened <see cref="BaseWindow"/> instances automatically.
/// </summary>
public sealed class UiAnimationSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private readonly HashSet<Control> _seenWindows = new();
    private readonly Dictionary<Control, float> _activeAnims = new();
    private readonly List<Control> _removeQueue = new();

    private const float DefaultDuration = 0.22f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        DetectNewWindows();
        UpdateAnimations(frameTime);
    }

    private void DetectNewWindows()
    {
        foreach (var child in _uiManager.WindowRoot.Children)
        {
            if (child is not BaseWindow)
                continue;

            if (!_seenWindows.Add(child))
                continue;

            // New window — start fade-in
            _activeAnims[child] = 0f;
            child.ModulateSelf = new Color(1f, 1f, 1f, 0f);
        }

        _seenWindows.RemoveWhere(c => c.Disposed);
    }

    private void UpdateAnimations(float frameTime)
    {
        foreach (var (control, elapsed) in _activeAnims)
        {
            if (control.Disposed)
            {
                _removeQueue.Add(control);
                continue;
            }

            var t = Math.Min(elapsed / DefaultDuration, 1f);
            // Ease-out-cubic
            var eased = 1f - MathF.Pow(1f - t, 3f);

            control.ModulateSelf = new Color(1f, 1f, 1f, eased);

            if (t >= 1f)
            {
                control.ModulateSelf = Color.White;
                _removeQueue.Add(control);
            }
            else
            {
                _activeAnims[control] = elapsed + frameTime;
            }
        }

        foreach (var control in _removeQueue)
            _activeAnims.Remove(control);
        _removeQueue.Clear();
    }

    /// <summary>
    /// Trigger a fade-in on any arbitrary control (popups, tooltips, etc.).
    /// </summary>
    public void FadeIn(Control control)
    {
        if (control.Disposed)
            return;
        _activeAnims[control] = 0f;
        control.ModulateSelf = new Color(1f, 1f, 1f, 0f);
    }
}
