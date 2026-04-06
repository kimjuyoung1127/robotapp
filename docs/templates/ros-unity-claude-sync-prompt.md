# ROS-Unity Claude Sync Prompt



## Copy-Paste Prompt



해야 할 일:
1. 내가 적은 내용을 5줄 이내로 요약
2. 내가 직접 확인한 사실과 추측을 분리
3. 빠진 중요한 정보가 있으면 최대 5개만 체크리스트로 제안
4. 다음 작업 1~3개를 우선순위 순으로 정리
5. 내가 그대로 팀에 공유할 수 있게 문장을 너무 길지 않게 다듬기

출력 형식:
- Summary
- Confirmed
- Assumptions
- Missing Info
- Next Steps

아래는 sync note:

[여기에 ros-unity-sync-template 내용을 붙여넣기]
```

## Optional Follow-Up Prompt



## Rule Of Thumb

- Claude에게 처음부터 큰 설계 문서를 쓰라고 하지 않는다.
- 먼저 `요약 + 누락 정보 + 다음 단계`만 받는다.
- 문서가 길어지면 다음 사람이 안 읽기 때문에 1페이지 안쪽으로 유지한다.
