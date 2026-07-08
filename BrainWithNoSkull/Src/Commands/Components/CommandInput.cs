namespace BrainWithNoSkull.Commands.Components;

public class CommandInput {

    private readonly string _input;
    private int _index;

    public CommandInput(string input){
        _input = input;
    }

    public int RemainingLength(){
        return _input.Length - _index;
    }
    
    public string Peek(int length){
        return _input.Substring(_index, length);
    }

    public void Advance(int length){
        _index += length;
    }

    public void Reset(){
        _index = 0;
    }
    

}