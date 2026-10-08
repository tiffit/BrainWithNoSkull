namespace BrainWithNoSkull.Util;

public class RingBuffer<T> {

    private readonly T[] _buffer;
    private int _ptr = 0;

    public RingBuffer(int size){
        _buffer = new T[size];
    }

    public void Add(T val){
        _buffer[_ptr] = val;
        _ptr = (_ptr + 1) % _buffer.Length;
    }

    public T Get(int index){
        index = (_ptr + index) % _buffer.Length;
        return _buffer[index];
    }

}