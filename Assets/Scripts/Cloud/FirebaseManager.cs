using UnityEngine;
using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;

/// <summary>
/// FirebaseManager — Usa el SDK oficial de Firebase.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager _instance;
    public static FirebaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("FirebaseManager");
                _instance = go.AddComponent<FirebaseManager>();
            }
            return _instance;
        }
    }

    // TODO: Reemplaza esto con el Web Client ID de tu Firebase Console
    private const string GOOGLE_WEB_CLIENT_ID = "69915361669-odl92trdthddrj7e13u9vfhcoghr6lor.apps.googleusercontent.com";

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }

    public string UserID => User?.UserId ?? "";
    public string UserEmail => User?.Email ?? "";
    public bool IsLoggedIn => User != null;
    public bool IsAnonymous => User != null && string.IsNullOrEmpty(User.Email);

    public event Action OnLoginSuccess;
    public event Action<string> OnLoginFailed;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                Auth.StateChanged += AuthStateChanged;
                AuthStateChanged(this, null);
                Debug.Log("[Firebase] Inicializado correctamente.");
            }
            else
            {
                Debug.LogError($"[Firebase] No se pudieron resolver todas las dependencias: {dependencyStatus}");
            }
        });
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (Auth.CurrentUser != User)
        {
            bool signedIn = User != Auth.CurrentUser && Auth.CurrentUser != null;
            if (!signedIn && User != null)
            {
                Debug.Log("[Firebase] Sesión cerrada");
            }
            User = Auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log($"[Firebase] Sesión iniciada: {User.UserId}");
                OnLoginSuccess?.Invoke();
            }
        }
    }

    // ── Login con Google ──────────────────────────────────────────────────────────
    public void LoginWithGoogle(Action onSuccess = null, Action<string> onError = null)
    {
        if (Auth == null) return;

        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = GOOGLE_WEB_CLIENT_ID
        };

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        signIn.ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                onError?.Invoke("Inicio de sesión cancelado.");
                return;
            }
            if (task.IsFaulted)
            {
                onError?.Invoke("Error en inicio de sesión de Google.");
                return;
            }

            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    onError?.Invoke(ParseError(authTask.Exception));
                    return;
                }
                Debug.Log($"[Firebase] Login con Google OK: {authTask.Result.Email}");
                onSuccess?.Invoke();
            });
        });
    }

    // ── Login anónimo ─────────────────────────────────────────────────────────────
    public void LoginAnonymous()
    {
        if (Auth == null) return;
        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string err = ParseError(task.Exception);
                OnLoginFailed?.Invoke(err);
                return;
            }
            // El evento AuthStateChanged disparará OnLoginSuccess
        });
    }

    // ── Registro con email ────────────────────────────────────────────────────────
    public void RegisterWithEmail(string email, string password, Action onSuccess = null, Action<string> onError = null)
    {
        if (Auth == null) return;
        Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string err = ParseError(task.Exception);
                onError?.Invoke(err);
                OnLoginFailed?.Invoke(err);
                return;
            }
            Debug.Log($"[Firebase] Registro OK: {email}");
            onSuccess?.Invoke();
        });
    }

    // ── Login con email ───────────────────────────────────────────────────────────
    public void LoginWithEmail(string email, string password, Action onSuccess = null, Action<string> onError = null)
    {
        if (Auth == null) return;
        Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string err = ParseError(task.Exception);
                onError?.Invoke(err);
                OnLoginFailed?.Invoke(err);
                return;
            }
            Debug.Log($"[Firebase] Login OK: {email}");
            onSuccess?.Invoke();
        });
    }

    // ── Reset password ────────────────────────────────────────────────────────────
    public void SendPasswordReset(string email, Action onSuccess = null, Action<string> onError = null)
    {
        if (Auth == null) return;
        Auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onError?.Invoke(ParseError(task.Exception));
                return;
            }
            onSuccess?.Invoke();
        });
    }

    // ── Cerrar sesión ─────────────────────────────────────────────────────────────
    public void Logout()
    {
        if (Auth == null) return;
        Auth.SignOut();
        User = null;
        LoginAnonymous(); // Al cerrar sesión con cuenta, volvemos a anónimo
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────
    private string ParseError(AggregateException aggregateException)
    {
        if (aggregateException == null) return "Error desconocido.";
        
        string errorMsg = "Error de conexión.";
        foreach (var inner in aggregateException.InnerExceptions)
        {
            if (inner is FirebaseException firebaseEx)
            {
                var errorCode = (AuthError)firebaseEx.ErrorCode;
                switch (errorCode)
                {
                    case AuthError.EmailAlreadyInUse: errorMsg = "Este email ya está registrado."; break;
                    case AuthError.WeakPassword: errorMsg = "La contraseña es muy débil."; break;
                    case AuthError.InvalidEmail: errorMsg = "El email no es válido."; break;
                    case AuthError.UserNotFound: errorMsg = "No existe cuenta con ese email."; break;
                    case AuthError.WrongPassword: errorMsg = "Contraseña incorrecta."; break;
                    case AuthError.TooManyRequests: errorMsg = "Demasiados intentos. Intenta más tarde."; break;
                    default: errorMsg = "Error: " + errorCode.ToString(); break;
                }
            }
        }
        return errorMsg;
    }
}
