using AutoMapper;
using ManejoPresupuestos.Models;

namespace ManejoPresupuestos.Servicios
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
            CreateMap<Transaccion, TransaccionActualizacionViewModel>().ReverseMap();
        }
    }
}
