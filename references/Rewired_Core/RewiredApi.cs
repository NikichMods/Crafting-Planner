// SPDX-License-Identifier: MPL-2.0
// Verified compile signatures only. Never distribute this reference assembly.
namespace Rewired
{
    public enum ControllerType { Keyboard = 0, Mouse = 1, Joystick = 2, Custom = 20 }
    public interface IControllerTemplate { }
    public interface IControllerTemplateButton { bool justPressed { get; } }
    public interface IControllerTemplateThumbStick { IControllerTemplateButton press { get; } }
    public class Controller
    {
        public bool isConnected { get { return false; } }
        public T GetTemplate<T>() where T : class, IControllerTemplate { return null; }
    }
    public class Player
    {
        public ControllerHelper controllers;
        public class ControllerHelper { public Controller GetLastActiveController(ControllerType type) { return null; } }
    }
    public static class ReInput
    {
        public static bool isReady { get { return false; } }
        public static PlayerHelper players { get { return null; } }
        public class PlayerHelper { public Player GetPlayer(int id) { return null; } }
    }
}
