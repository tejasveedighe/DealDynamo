﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DealDynamo.Data;
using DealDynamo.Models;
using DealDynamo.Services;
using Microsoft.AspNetCore.Authorization;
using DealDynamo.Models.CategoryViewModels;

namespace DealDynamo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        // GET: Categories
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;

            var categories = categoryRepository.GetAllCategories();

            int pageNumber = page ?? 1;

            var viewModel = new CategoryListViewModel
            {
                Categories = categories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(categories.Count() / (double)pageSize)
            };
            return View(viewModel);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || categoryRepository == null)
            {
                return NotFound();
            }

            var category = categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Checkout
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Checkout
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepository.AddCategory(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || categoryRepository == null)
            {
                return NotFound();
            }

            var category = categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    categoryRepository.UpdateCategory(category);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (categoryRepository.GetCategoryById(category.Id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || categoryRepository == null)
            {
                return NotFound();
            }

            var category = categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (categoryRepository == null)
            {
                return Problem("Entity set 'DealDynamoContext.Category'  is null.");
            }

            categoryRepository.DeleteCategory(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
