// SPDX-FileCopyrightText: 2026 Freak
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Client.Animations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Content.Client.UserInterface.Controls;

namespace Content.Client._FreakyStation.UI;

/// <summary>
/// Animated UI transitions. Non-blocking — if anything fails, windows stay fully visible.
/// </summary>
public sealed class UiAnimationSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private readonly Dictionary<Control, float> _pulsingControls = new();
    private readonly List<Control> _pulseRemove = new();

    private const float EntranceDuration = 0.25f;
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

    public override void Initialize()
    {
        base.Initialize();
        _uiManager.WindowRoot.OnChildAdded += OnWindowChildAdded;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdatePulse(frameTime);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _uiManager.WindowRoot.OnChildAdded -= OnWindowChildAdded;
    }

    private void OnWindowChildAdded(Control child)
    {
        // Only animate DefaultWindow and FancyWindow
        if (child is not (DefaultWindow or FancyWindow))
            return;

        try
        {
            child.StopAnimation(EntranceKey);
            child.Modulate = Color.White.WithAlpha(0f);
            child.PlayAnimation(EntranceAnim, EntranceKey);
            // Safety: if animation doesn't complete for any reason, force-reset after duration
            Timer.Spawn(TimeSpan.FromSeconds(EntranceDuration + 0.1f), () =>
            {
                if (!child.Disposed && child.Modulate != Color.White)
                    child.Modulate = Color.White;
            });
        }
        catch
        {
            // Never block window opening — if animation fails, force visible
            child.Modulate = Color.White;
        }
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
        try
        {
            control.StopAnimation(HoverKey);
            control.PlayAnimation(HoverInAnim, HoverKey);
        }
        catch { /* ignore */ }
    }

    public static void AnimateHoverOut(Control control)
    {
        if (control.Disposed)
            return;
        try
        {
            control.StopAnimation(HoverKey);
            control.PlayAnimation(HoverOutAnim, HoverKey);
        }
        catch { /* ignore */ }
    }

    public static void AnimateClickPop(Control control)
    {
        if (control.Disposed)
            return;
        try
        {
            control.StopAnimation(ClickKey);
            control.PlayAnimation(ClickPopAnim, ClickKey);
        }
        catch { /* ignore */ }
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
