using Gargar.Common.Application.Interfaces;
using Gargar.Common.Application.Mapper;
using Gargar.Common.Application.POCO;
using Gargar.Common.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gargar.Common.Application.Service
{
    internal class ImageService : BaseUoWService<Image,Guid,ImageDTO, ImageMappers> , IImageService
    {
        public ImageService(IUnitOfWork uow) : base(uow)
        {

        }
    }
}
