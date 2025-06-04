using Gargar.Common.Application.POCO;
using Gargar.Common.Domain.Models;
using Gargar.Common.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gargar.Common.Application.Interfaces;

public interface IImageService : IUoWService<Image,Guid,ImageDTO>
{

}
