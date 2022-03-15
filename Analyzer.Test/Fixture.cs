using MockAnalyzer.Test.Registration.Step1;
using MockAnalyzer.Test.Registration.Step2;

namespace MockAnalyzer.Test.Registration
{
    public class RegistrationOrchestrator
    {
        public void GoThroughFlow()
        {
            Screen.Open();

            Screen2.Open();
        }
    }
}


namespace MockAnalyzer.Test.Registration.Step1
{
    using MockAnalyzer.Test.Login;
    public class Screen
    {
        private static RegistrationStatus GetRegistrationStatus()
        {
            return RegistrationStatus.NotRegistered;
        }
        public static void Open()
        {
            // open screen 1
            if (GetRegistrationStatus() == RegistrationStatus.Registered)
            {
                LoginScreen.ShowLoginScreen();
            }

        }
    }

    public enum RegistrationStatus
    {
        NotRegistered,
        Registered
    }
}

namespace MockAnalyzer.Test.Registration.Step2
{
    public class Screen2
    {
        public static void Open()
        {
            // open screen 2
        }
    }
}

namespace MockAnalyzer.Test.Login
{
    public class LoginScreen
    {
        public static void ShowLoginScreen()
        {
            //
        }
    }
}
