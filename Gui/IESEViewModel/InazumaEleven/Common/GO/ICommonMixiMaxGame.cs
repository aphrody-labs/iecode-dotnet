using IESEViewModel.InazumaEleven.Logic;

namespace IESEViewModel.InazumaEleven.Common.GO;

public interface ICommonMixiMaxGame : ICommonGame
{
    static abstract List<BestMatch> BestMatches { get; }

    static abstract Dictionary<int, List<PlayRecord>> PlayRecords { get; }

    public static byte[] NewBinary(List<PlayRecord> listPlayRecord)
    {
        byte[] binary = new byte[listPlayRecord.Count];

        for (int i = 0; i < listPlayRecord.Count; i++)
        {
            if (listPlayRecord[i].Unlocked == false)
            {
                binary[i] = 0x00;
            }
            else
            {
                binary[i] = 0x01;
            }
        }

        return binary;
    }
}