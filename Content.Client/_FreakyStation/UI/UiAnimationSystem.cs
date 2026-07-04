// SPDX-FileCopyrightText: 2026 Freak
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Client.Animations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._FreakyStation.UI;

/// <summary>
/// Animated UI transitions using RobustToolbox's built-in Animation system.
/// <list type="bullet">
/// <item>Window entrance: fade-in from transparent (300ms)</item>
/// <item>Button hover: smooth modulate dim/brighten (120ms)</item>
/// <item>Click pop: quick dim-and-release (150ms)</item>
/// <item>Accent pulse: sine-wave modulate oscillation for highlighted elements</item>
/// </list>
/// Only applies to <see cref="DefaultWindow"/> and <see cref="FancyWindow"/> —
/// lightweight popup-like <see cref="BaseWindow"/> subclasses are left alone.
/// </summary>
public sealed class UiAnimationSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private readonly HashSet<Control> _seenWindows = new();
    private readonly Dictionary<Control, float> _pulsingControls = new();
    private readonly List<Control> _pulseRemove = new();

    private const float EntranceDuration = 0.30f;
    private const float HoverDuration = 0.12f;
    private const float ClickDuration = 0.15f;
    private const float PulsePeriod = 1.6f;

    private const string HoverKey = "freaky-hover";
    private const string ClickKey = "freaky-click";
    private const string EntranceKey = "freaky-entrance";

    private static readonly Color HoverTint = new(0.88f, 0.88f, 0.88f, 1f);
    private static readonly Color PressTint = new(0.78f, 0.78f, 0.78f, 1f);

    private static readonly Animation EntranceAnim = new()
    {
        Length = TimeSpan.FromSeconds(EntranceDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, EntranceDuration),
                }
            },
        }
    };

    private static readonly Animation HoverInAnim = new()
    {
        Length = TimeSpan.FromSeconds(HoverDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White, 0f),
                    new AnimationTrackProperty.KeyFrame(HoverTint, HoverDuration),
                }
            },
        }
    };

    private static readonly Animation HoverOutAnim = new()
    {
        Length = TimeSpan.FromSeconds(HoverDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(HoverTint, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, HoverDuration),
                }
            },
        }
    };

    private static readonly Animation ClickPopAnim = new()
    {
        Length = TimeSpan.FromSeconds(ClickDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(PressTint, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, ClickDuration),
                }
            },
        }
    };

    /// <summary>
    /// Check if a control is a "full" window that should get entrance animations.
    /// Excludes lightweight popup-like windows.
    /// </summary>
    private static bool IsFullWindow(Control control)
    {
        return control is DefaultWindow or FancyWindow;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        DetectNewWindows();
        UpdatePulse(frameTime);
    }

    private void DetectNewWindows()
    {
        foreach (var child in _uiManager.WindowRoot.Children)
        {
            if (!IsFullWindow(child))
                continue;

            if (!_seenWindows.Add(child))
                continue;

            // Entrance: fade-in from transparent
            child.Modulate = Color.White.WithAlpha(0f);
            child.PlayAnimation(EntranceAnim, EntranceKey);
        }

        _seenWindows.RemoveWhere(c => c.Disposed);
    }

    private void UpdatePulse(float frameTime)
    {
        foreach (var (control, phase) in _pulsingControls)
        {
            if (control.Disposed)
            {
                _pulseRemove.Add(control);
                continue;
            }

            var t = (phase + frameTime) % PulsePeriod;
            _pulsingControls[control] = t;

            var pulse = 0.85f + 0.15f * MathF.Sin(t * MathF.Tau / PulsePeriod);
            control.Modulate = new Color(pulse, pulse, pulse, 1f);
        }

        foreach (var control in _pulseRemove)
            _pulsingControls.Remove(control);
        _pulseRemove.Clear();
    }

    public static void AnimateHoverIn(Control control)
    {
        if (control.Disposed)
            return;
        control.StopAnimation(HoverKey);
        control.PlayAnimation(HoverInAnim, HoverKey);
    }

    public static void AnimateHoverOut(Control control)
    {
        if (control.Disposed)
            return;
        control.StopAnimation(HoverKey);
        control.PlayAnimation(HoverOutAnim, HoverKey);
    }

    public static void AnimateClickPop(Control control)
    {
        if (control.Disposed)
            return;
        control.StopAnimation(ClickKey);
        control.PlayAnimation(ClickPopAnim, ClickKey);
    }

    public void StartPulse(Control control)
    {
        if (control.Disposed)
            return;
        _pulsingControls[control] = 0f;
    }

    public void StopPulse(Control control)
    {
        if (_pulsingControls.Remove(control))
            control.Modulate = Color.White;
    }
}
