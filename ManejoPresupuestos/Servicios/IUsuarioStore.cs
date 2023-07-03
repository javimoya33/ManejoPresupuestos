using ManejoPresupuestos.Models;
using Microsoft.AspNetCore.Identity;

namespace ManejoPresupuestos.Servicios
{
    public interface IUsuarioStore
    {
        Task<IdentityResult> CreateAsync(Usuario user, CancellationToken cancellationToken);
        Task<IdentityResult> DeleteAsync(Usuario user, CancellationToken cancellationToken);
        void Dispose();
        Task<Usuario> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
        Task<Usuario> FindByIdAsync(string userId, CancellationToken cancellationToken);
        Task<Usuario> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken);
        Task<string> GetEmailAsync(Usuario user, CancellationToken cancellationToken);
        Task<bool> GetEmailConfirmedAsync(Usuario user, CancellationToken cancellationToken);
        Task<string> GetNormalizedEmailAsync(Usuario user, CancellationToken cancellationToken);
        Task<string> GetNormalizedUserNameAsync(Usuario user, CancellationToken cancellationToken);
        Task<string> GetPasswordHashAsync(Usuario user, CancellationToken cancellationToken);
        Task<string> GetUserIdAsync(Usuario user, CancellationToken cancellationToken);
        Task<string> GetUserNameAsync(Usuario user, CancellationToken cancellationToken);
        Task<bool> HasPasswordAsync(Usuario user, CancellationToken cancellationToken);
        Task SetEmailAsync(Usuario user, string email, CancellationToken cancellationToken);
        Task SetEmailConfirmedAsync(Usuario user, bool confirmed, CancellationToken cancellationToken);
        Task SetNormalizedEmailAsync(Usuario user, string normalizedEmail, CancellationToken cancellationToken);
        Task SetNormalizedUserNameAsync(Usuario user, string normalizedName, CancellationToken cancellationToken);
        Task SetPasswordHashAsync(Usuario user, string passwordHash, CancellationToken cancellationToken);
        Task SetUserNameAsync(Usuario user, string userName, CancellationToken cancellationToken);
        Task<IdentityResult> UpdateAsync(Usuario user, CancellationToken cancellationToken);
    }
}