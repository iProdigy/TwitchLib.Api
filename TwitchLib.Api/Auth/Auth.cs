﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Core.Interfaces;

namespace TwitchLib.Api.Auth
{
    /// <summary>These endpoints fall outside of v5 and Helix, and relate to Authorization</summary>
    public class Auth : ApiBase
    {
        public Auth(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        {
        }

        /// <summary>
        ///     <para>[ASYNC] Refreshes an expired auth token</para>
        ///     <para>ATTENTION: Client Secret required. Never expose it to consumers!</para>
        ///     <para>Throws a BadRequest Exception if the request fails due to a bad refresh token</para>
        /// </summary>
        /// <returns>A RefreshResponse object that holds your new auth and refresh token and the list of scopes for that token</returns>
        public Task<RefreshResponse> RefreshAuthTokenAsync(string refreshToken, string clientSecret, string clientId = null)
        {
            var internalClientId = clientId ?? Settings.ClientId;

            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new BadParameterException("The refresh token is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new BadParameterException("The client secret is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            if (string.IsNullOrWhiteSpace(internalClientId))
                throw new BadParameterException("The clientId is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            var getParams = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", internalClientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                };

            return TwitchPostGenericAsync<RefreshResponse>("/oauth2/token", ApiVersion.Void, null, getParams, customBase: "https://id.twitch.tv");
        }

        /// <summary>
        /// Generates an authorization code URL. Please see OAuth authorization code flow https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#oauth-authorization-code-flow.
        /// </summary>
        /// <param name="redirectUri">Your registered redirect URI. This must exactly match the redirect URI registered in the prior, Registration step.</param>
        /// <param name="scopes">Space-separated list of scopes.</param>
        /// <param name="forceVerify">Specifies whether the user should be re-prompted for authorization. If this is true, the user always is prompted to confirm authorization. This is useful to allow your users to switch Twitch accounts, since there is no way to log users out of the API. Default: false (a given user sees the authorization page for a given set of scopes only the first time through the sequence).</param>
        /// <param name="state">Your unique token, generated by your application. This is an OAuth 2.0 opaque value, used to avoid CSRF attacks. This value is echoed back in the response. We strongly recommend you use this.</param>
        /// <param name="clientId">Your client ID.</param>
        /// <returns>A URL encoded string that can be used to generate a user authorization code.</returns>
        /// <exception cref="BadParameterException">Thrown when any of the required parameters are not valid.</exception>
        public string GetAuthorizationCodeUrl(string redirectUri, IEnumerable<AuthScopes> scopes, bool forceVerify = false, string state = null, string clientId = null)
        {
            var internalClientId = clientId ?? Settings.ClientId;

            string scopesStr = null;
            foreach (var scope in scopes)
                if (scopesStr == null)
                    scopesStr = Core.Common.Helpers.AuthScopesToString(scope);
                else
                    scopesStr += $"+{Core.Common.Helpers.AuthScopesToString(scope)}";

            if (string.IsNullOrWhiteSpace(internalClientId))
                throw new BadParameterException("The clientId is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            return "https://id.twitch.tv/oauth2/authorize?" +
                   $"client_id={internalClientId}&" +
                   $"redirect_uri={System.Web.HttpUtility.UrlEncode(redirectUri)}&" +
                   "response_type=code&" +
                   $"scope={scopesStr}&" +
                   $"state={state}&" +
                   $"force_verify={forceVerify}";
        }

        /// <summary>
        ///     <para>[ASYNC] Uses an authorization code to generate an access token</para>
        ///     <para>ATTENTION: Client Secret required. Never expose it to consumers!</para>
        ///     <para>Throws a BadRequest Exception if the request fails due to a bad code token</para>
        /// </summary>
        /// <param name="code">The OAuth 2.0 authorization code is a 30-character, randomly generated string. Used in the request made to the token endpoint in exchange for an access token.</param>
        /// <param name="clientSecret">Required for API access.</param>
        /// <param name="redirectUri">The URI the user was redirected to. This URI must be registered with your twitch app or extension.</param>
        /// <param name="clientId">The client ID of your app or extension.</param>
        /// <returns>A RefreshResponse object that holds your new auth and refresh token and the list of scopes for that token</returns>
        public Task<AuthCodeResponse> GetAccessTokenFromCodeAsync(string code, string clientSecret, string redirectUri, string clientId = null)
        {
            var internalClientId = clientId ?? Settings.ClientId;

            if (string.IsNullOrWhiteSpace(code))
                throw new BadParameterException("The code is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new BadParameterException("The client secret is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            if (string.IsNullOrWhiteSpace(redirectUri))
                throw new BadParameterException("The redirectUri is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            if (string.IsNullOrWhiteSpace(internalClientId))
                throw new BadParameterException("The clientId is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            var getParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", internalClientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            };

            return TwitchPostGenericAsync<AuthCodeResponse>("/oauth2/token", ApiVersion.V5, null, getParams, customBase: "https://id.twitch.tv");
        }

        /// <summary>
        /// Checks the validation of the Settings.AccessToken or passed in AccessToken. If invalid, a null response is returned
        /// </summary>
        /// <param name="accessToken">Optional access token to check validation on</param>
        /// <returns>ValidateAccessTokenResponse</returns>
        public async Task<ValidateAccessTokenResponse> ValidateAccessTokenAsync(string accessToken = null)
        {
            try
            {
                return await TwitchGetGenericAsync<ValidateAccessTokenResponse>("/oauth2/validate", ApiVersion.Void, accessToken: accessToken, customBase: "https://id.twitch.tv");
            } catch(BadScopeException)
            {
                // BadScopeException == 401, which is surfaced when token is invalid
                return null;
            }
        }
    }
}
