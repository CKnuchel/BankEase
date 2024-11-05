// Verhindert das einfügen des Buchstaben 'E', sowie der -/+ Zeichen
function preventInvalidNumberInputs(event)
{
    if(["e", "E", "+", "-"].includes(event.key))
    {
        event.preventDefault();
    }
}