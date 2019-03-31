using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChallengeKit.Pattern
{
    public interface IMessenger
    {
        void ProcSend(string Command, params object[] Objs);
        void ProcReceive(string Command, params object[] Objs);
        // 추가 고려 구현 대상들 : 연결되었는가? 인자로는 채널 id, 그리고 해당 메세지를 구독하기.
    }

    // 진짜 네트워크 기능까지 고려한 코드를 설계 할때면 이 SampleMessenger를 실 구현하고, 사용하는 게임에는 이걸 상속받아서 활용한다.
    // 실 구현에서는 이런식의 가변적인 파람 설계는 불가능할듯하다.

}
