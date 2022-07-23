using FakeXiecheng.api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeXiecheng.api.Services
{
    public class MockTouristRouteRepository
    {
        private List<TouristRoute> _routes;

        public MockTouristRouteRepository()
        {
            if(_routes == null)
            {
                InitializeTouristRoutes();
            }
        }

        private void InitializeTouristRoutes()
        {
            _routes = new List<TouristRoute>()
            {
                new TouristRoute
                {
                    Id=Guid.NewGuid(),
                    Title="黄山",
                    Description="黄山真好玩",
                    OriginalPrice=1299,
                    Features="<p>吃住行游购娱</p>",
                    Fees="<p>交通费自理</p>",
                    Notes="<p>小心危险</p>"
                },
                new TouristRoute
                {
                    Id=Guid.NewGuid(),
                    Title="华山",
                    Description="华山真好玩",
                    OriginalPrice=1299,
                    Features="<p>吃住行游购娱</p>",
                    Fees="<p>交通费自理</p>",
                    Notes="<p>小心危险</p>"
                }
            };
        }
        public TouristRoute GetTouristRoute(Guid touristRouteId)
        {
            return _routes.FirstOrDefault(n => n.Id == touristRouteId);
        }

        public bool TouristRouteExists(Guid touristRouteId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TouristRoutePicture> GetPicturesByTouristRouteId(Guid touristRouteId)
        {
            throw new NotImplementedException();
        }

        public TouristRoutePicture GetPicture(int pictureId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TouristRoute> GetTouristRoutes(string keyword, string operatorType, int? ratingValue)
        {
            throw new NotImplementedException();
        }

        public void AddTouristRoute(TouristRoute touristRoute)
        {
            throw new NotImplementedException();
        }
    }
}
