using BlazingQuiz.Shared;
using BlazingQuiz.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace BlazingQuiz.Web.Auth;

public class QuizAuthProvider : AuthenticationStateProvider
{
    private const string AuthType = "quiz-auth";
    private const string UserDataKey = "udata";

    private Task<AuthenticationState> _authStateTask;
    private readonly IJSRuntime _jSRuntime;

    public QuizAuthProvider(IJSRuntime jSRuntime)
    {
        _jSRuntime = jSRuntime;
        SetAuthStateTask();
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        _authStateTask;

    public LoggedInUser User { get; private set; }
    public bool IsLoggedIn => User?.Id > 0;

    public async Task SetLoginAsync(LoggedInUser user)
    {
        User = user;
        SetAuthStateTask();
        NotifyAuthenticationStateChanged(_authStateTask);
        await _jSRuntime.InvokeVoidAsync("localStorage.setItem", UserDataKey, user.ToJson());
    }
    
    public bool IsInitializing { get; private set; } = true;
    public async Task InitializeAsync() 
    {
        try
        {
            var udata = await _jSRuntime.InvokeAsync<string?>("localStorage.getItem", UserDataKey);
            if (string.IsNullOrWhiteSpace(udata))
            {
                return;
            }
            var user = LoggedInUser.LoadForm(udata);
            if (user == null || user.Id == 0)
            {
                return;
            }
            await SetLoginAsync(user);
        }
        catch (Exception e)
        {
            // TODO : Fix the error
        }
        finally
        {
            IsInitializing = false;
        } 
    }

    public async Task SetLogoutAsync() 
    {
        User = null;
        SetAuthStateTask();
        NotifyAuthenticationStateChanged(_authStateTask);
        await _jSRuntime.InvokeVoidAsync("localStorage.removeItem", UserDataKey);

    }

    private void SetAuthStateTask()
    {
        if (IsLoggedIn)
        {
            var identity = new ClaimsIdentity(User.ToClaims(), AuthType);
            var principle = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principle);

            _authStateTask = Task.FromResult(authState);
        }
        else
        {
            var identity = new ClaimsIdentity();
            var principle = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principle);

            _authStateTask = Task.FromResult(authState);
        }
    }
}