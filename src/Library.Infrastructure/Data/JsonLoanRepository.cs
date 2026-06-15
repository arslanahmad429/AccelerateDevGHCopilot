using Library.ApplicationCore;
using Library.ApplicationCore.Entities;

namespace Library.Infrastructure.Data;

public class JsonLoanRepository : ILoanRepository
{
    private readonly JsonData _jsonData;

    public JsonLoanRepository(JsonData jsonData)
    {
        _jsonData = jsonData;
    }

    public async Task<Loan?> GetLoan(int id)
    {
        await _jsonData.EnsureDataLoaded();

        foreach (Loan loan in _jsonData.Loans!)
        {
            if (loan.Id == id)
            {
                Loan populated = _jsonData.GetPopulatedLoan(loan);
                return populated;
            }
        }
        return null;
    }

    public async Task UpdateLoan(Loan loan)
    {
        Loan? existingLoan = null;
        foreach (Loan l in _jsonData.Loans!)
        {
            if (l.Id == loan.Id)
            {
                existingLoan = l;
                break;
            }
        }

        if (existingLoan != null)
        {
            existingLoan.BookItemId = loan.BookItemId;
            existingLoan.PatronId = loan.PatronId;
            existingLoan.LoanDate = loan.LoanDate;
            existingLoan.DueDate = loan.DueDate;
            existingLoan.ReturnDate = loan.ReturnDate;

            await _jsonData.SaveLoans(_jsonData.Loans!);

            await _jsonData.LoadData();
        }
    }

    public async Task<List<Loan>> SearchLoansByBookTitle(string title)
    {
        await _jsonData.EnsureDataLoaded();

        string normalizedTitle = title.Trim().ToLowerInvariant();
        var matchingBookItemIds = new HashSet<int>();

        foreach (BookItem bookItem in _jsonData.BookItems!)
        {
            BookItem populatedBookItem = _jsonData.GetPopulatedBookItem(bookItem);
            if (populatedBookItem.Book?.Title?.ToLowerInvariant().Contains(normalizedTitle) == true)
            {
                matchingBookItemIds.Add(populatedBookItem.Id);
            }
        }

        List<Loan> matchingLoans = new List<Loan>();
        foreach (Loan loan in _jsonData.Loans!)
        {
            if (matchingBookItemIds.Contains(loan.BookItemId))
            {
                matchingLoans.Add(_jsonData.GetPopulatedLoan(loan));
            }
        }

        return matchingLoans;
    }
}