﻿using bulkey.DataAccess.Repository.IRepository;
using bulkey.Models;
using bulkey.Models.ViewModels;
using bulkey.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace bulkey.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        //public IActionResult Index()
        //{

        //    IEnumerable<Product> productList=_unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
        //    return View(productList);
        //}



        public async Task<IActionResult> Index(string SearchString)
        {

            ViewData["Filter"]=SearchString;
            var products =from p in _unitOfWork.Product.GetAll() select p;
            if (!String.IsNullOrEmpty(SearchString))
            {
                products = products.Where(p => p.Title.Contains(SearchString));
            }
            return View(products);

            //IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            //return View(productList);
        }

        public IActionResult Details(int pruductId)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId = pruductId,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == pruductId, includeProperties: "Category,CoverType")

        };   
            return View(cartObj);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DetailsAsync(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);


            if (cartFromDb == null)
            {

                _unitOfWork.ShoppingCart.Add(shoppingCart);
               
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
            }
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                _unitOfWork.Save();
            }


            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult ContactMail()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}