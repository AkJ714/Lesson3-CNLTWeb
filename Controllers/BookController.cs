using Lesson3_CNLTWeb.Models;
using Lesson3_CNLTWeb.Data;
using Microsoft.AspNetCore.Mvc;

namespace Lesson3_CNLTWeb.Controllers
{
    public class BookController : Controller
    {
        public IActionResult Index(string? search, string? sortOrder)
        {
            var books = BookRepository.GetAllBooks(search, sortOrder);
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentSort"] = sortOrder;
            return View(books);
        }

        public IActionResult Detail(int id)
        {
            var book = BookRepository.GetBookById(id);
            if (book == null) return NotFound();
            return View(book);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (!ModelState.IsValid) return View(book);
            BookRepository.AddBook(book);
            TempData["SuccessMessage"] = "Thêm sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var book = BookRepository.GetBookById(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Book book)
        {
            if (!ModelState.IsValid) return View(book);
            BookRepository.UpdateBook(book);
            TempData["SuccessMessage"] = "Cập nhật sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var book = BookRepository.GetBookById(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            BookRepository.DeleteBook(id);
            TempData["SuccessMessage"] = "Xóa sách thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
