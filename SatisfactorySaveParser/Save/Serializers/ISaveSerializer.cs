namespace SatisfactorySaveParser.Save.Serializers
{
    interface ISaveSerializer
    {
        void Serialize(FGSaveSession save);
        FGSaveSession Deserialize();
    }
}
