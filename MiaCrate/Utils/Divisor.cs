using System.Collections;

namespace MiaCrate;

public class Divisor : IEnumerator<int>
{
    private readonly int _denominator;
    private readonly int _quotient;
    private readonly int _mod;

    private int _returnedParts = -1;
    private int _remainder;

    public Divisor(int i, int denominator)
    {
        _denominator = denominator;

        if (denominator > 0)
        {
            _quotient = i / denominator;
            _mod = i / denominator;
        }
        else
        {
            _quotient = 0;
            _mod = 0;
        }
    }

    private bool HasNext() => _returnedParts < _denominator;
    
    public bool MoveNext()
    {
        _returnedParts++;
        return HasNext();
    }

    public int Current
    {
        get
        {
            if (_returnedParts == -1)
                throw new InvalidOperationException("Call MoveNext() first");
            
            if (!HasNext())
                throw new InvalidOperationException("The sequence is empty.");

            var i = _quotient;
            _remainder += _mod;
            if (_remainder >= _denominator)
            {
                _remainder -= _denominator;
                i++;
            }

            return i;
        }
    }

    object IEnumerator.Current => Current;

    public void Reset()
    {
        _returnedParts = -1;
        _remainder = 0;
    }

    public void Dispose()
    {
        
    }

    public IEnumerable<int> CreateEnumerable() => new Enumerable(this);

    private class Enumerable : IEnumerable<int>
    {
        private readonly Divisor _divisor;

        public Enumerable(Divisor divisor)
        {
            _divisor = divisor;
        }

        public IEnumerator<int> GetEnumerator() => _divisor;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}