/********************************************
-	    File Name: UIHUD
-	  Description: 角色头上的组件
-	 	   Author: lijing,<979477187@qq.com>
-     Create Date: Created by lijing on #CREATIONDATE#.
-Revision History: 
********************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameKit
{
    /// <summary>
    /// 初始化组件需要的数据
    /// </summary>
    public struct StructUIHUD
    {
        public long CurHP;
        public long MaxHP;
        public string ActorName;
        public GameObject FollowObject;     //跟随的对象
        public bool IsActiveMP;
        public bool IsActiveActorName;
        public Color NameColor;

        public SkillButtonData SkillButtonData;     //技能cd冷却显示在头顶,非必要,看项目需求
    }

    /// <summary>
    /// 技能按钮信息
    /// </summary>
    public struct SkillButtonData
    {
        public int index;           //技能序列号
        public uint skillId;        //技能ID
        public uint heroId;         //英雄ID，配置ID
        public uint uid;            //客户端分配的角色唯一ID
        public float currCDTime;    //当前技能CD
        public float maxCDTime;     //技能最大CD
    }

    public class UIHUD : MonoBehaviour
    {

        #region 变量

        [SerializeField]    UISprite _hp_fg;             //hp前景图片
        [SerializeField]    UISlider _hpBar;             //血条bar
        [SerializeField]    UISlider _mpBar;             //蓝条
        [SerializeField]    UILabel _actorName;          //单位名称
        [SerializeField]    UISprite _bubbleSprite;      //台词气泡背景框
        [SerializeField]    UILabel _label_bubble;       //显示台词
        [SerializeField]    UISprite _sp_actorMark;      //角色类型标志
        [SerializeField]    GameObject _sp_cloneScale;   //被克隆的刻度条

        Transform _followedTarget;    // 要跟随的目标（通常是场景里怪物或英雄的头顶上方的某个空子对象）
        Camera _gameCamera;           // 3D摄像机
        Camera _uiCamera;             // UI摄像机
        int _targetVisible = -1;      // 目标是否在视野内可见（这里不能用bool，也不能赋值为0或1）

        static string prefabPath = "Hud/UIHud"; // 预设路径

        List<GameObject> _scaleList = new List<GameObject>();    //血条上的刻度列表

        public static readonly int Param_min = 7;   //血量刻度计算参数

        public static int BaseHPScale = 500;          //由外部设置，我方最大血量单位/Param_min

        readonly int SCALE_MIN = 4;                 //最小刻度数量
        readonly int SCALE_MAX = 15;                //最大刻度数量

        StructUIHUD MyStructUIHUD;                  //组件所需基础数据
        #endregion

        // Use this for initialization
        void Start()
        {
           
        }


        public static UIHUD Create(StructUIHUD uihuid ,Transform parent)
        {
            // 实例化
            GameObject go = Instantiate(Resources.Load<GameObject>(prefabPath)) as GameObject;

            UIHUD monster = go.GetComponent<UIHUD>();
            monster.Init(uihuid, parent);

            return monster;
        }

        #region 接口

        void Init(StructUIHUD uihuid,Transform parent)
        {
            if (uihuid.FollowObject == null)
            {
                Debug.LogError("followObj==null");
                Destroy(gameObject);
                return;
            }

            if (parent == null)
            {
                Debug.LogError("parent is null");
                return;
            }

            MyStructUIHUD = uihuid;
            _followedTarget = MyStructUIHUD.FollowObject.transform;

            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            // 摄像机
            _gameCamera = NGUITools.FindCameraForLayer(MyStructUIHUD.FollowObject.layer);
            _uiCamera = NGUITools.FindCameraForLayer(parent.gameObject.layer);
            //设置当前hp进度条
            OnEventHPChange(MyStructUIHUD.CurHP, MyStructUIHUD.MaxHP);
            //设置名字
            SetName(MyStructUIHUD);
            //名字颜色
            SetNameColor(MyStructUIHUD.NameColor);
            //hp刻度条
            BaseHPScale = BaseHPScale == 0 ? (int)uihuid.MaxHP / UIHUD.Param_min : BaseHPScale;
            SetHPScale(BaseHPScale, (int)uihuid.MaxHP);
            //类型标识
            SetHeroFeature(uihuid);

            _mpBar.gameObject.SetActive(MyStructUIHUD.IsActiveMP);
        }

        public void SetName(StructUIHUD value)
        {
            _actorName.gameObject.SetActive(value.IsActiveActorName);
            _actorName.text = value.ActorName;
        }

        public void SetNameColor(Color actorColor)
        {
            if (actorColor!=null)
                _actorName.color = actorColor;
        }

        public void SetRoleMark(string value)
        {
            if (!_sp_actorMark.gameObject.activeSelf) _sp_actorMark.gameObject.SetActive(true);

            _sp_actorMark.spriteName = value;
        }

        public void SetActiveRoleMark(bool value)
        {
            if (_sp_actorMark.gameObject.activeSelf != value) _sp_actorMark.gameObject.SetActive(value);
        }

        public void RemoveSelf()
        {
            _followedTarget = null;
            _gameCamera = null;
            _uiCamera = null;
            _scaleList = null;

            // 销毁当前对象
            Destroy(gameObject);
        }

        /// <summary>
        /// 设置血条刻度
        /// </summary>
        /// <param name="baseMark"></param>
        public void SetHPScale(int baseScale, int hpMAX)
        {
            if (baseScale == 0 || hpMAX == 0)
            {
                Debug.LogError(string.Format("don't set hp Scale, baseScale:{0} hpMAX:{1}", baseScale, hpMAX));
                return;
            }

            if (hpMAX == MyStructUIHUD.MaxHP)
            {
                //上限相同不需要重设
                //return;
            }

            for (int i = 0; i < _scaleList.Count; i++)
            {
                Destroy(_scaleList[i]);
            }
            _scaleList.Clear();

            //计算刻度数量
            int markNumb = hpMAX / baseScale;
            markNumb = markNumb > SCALE_MAX ? SCALE_MAX : markNumb;
            markNumb = markNumb < SCALE_MIN ? SCALE_MIN : markNumb;
            //计算刻度坐标间距
            int dis = _hp_fg.width / (markNumb + 1);
            int p = (_hp_fg.width % (markNumb + 1)) / 2;//处理精度
            for (int i = 1; i <= markNumb; i++)
            {
                GameObject go = NGUITools.AddChild(_hp_fg.gameObject, _sp_cloneScale);
                go.SetActive(true);
                go.transform.localPosition = new Vector3(dis * i + p, 0, 0);

                _scaleList.Add(go);
            }

        }

        /// <summary>
        /// 设置英雄特性标志
        /// </summary>
        /// <param name="uihuid"></param>
        void SetHeroFeature(StructUIHUD uihuid)
        {
            
        }

        #endregion

        #region 逻辑

        /// <summary>
        /// 监听hp变化
        /// </summary>
        /// <param name="newHP"></param>
        /// <param name="maxHP"></param>
        public void OnEventHPChange(long newHP, long maxHP)
        {
            if (NGUITools.GetActive(_hpBar))
            {
                _hpBar.value = (float)newHP / maxHP;
            }
        }

        public void OnEventActorDie(ActorBehaviour actor)
        {
            RemoveSelf();
        }


        /// <summary>
        /// 监听角色释放技能
        /// </summary>
        /// <param name="skillid"></param>
        public void OnEventActorPlaySkill(uint skillid)
        {
            if (skillid ==  MyStructUIHUD.SkillButtonData.skillId)
            {
                MyStructUIHUD.SkillButtonData.currCDTime = MyStructUIHUD.SkillButtonData.maxCDTime;
            }
        }

        /*
        UIHudPanel MyHudPanel
        {
            get
            {
                UIHudPanel hudPanel = UI3System.findWindow<UIHudPanel>();
                if (hudPanel == null)
                {
                    hudPanel = UI3System.createWindow<UIHudPanel>();

                    if (hudPanel == null) return null;

                    hudPanel.show();
                }
                return hudPanel;
            }
        }
         */

        void Update()
        {
            if (_followedTarget == null || _uiCamera == null || _gameCamera == null)
            {
                RemoveSelf();
            }
            else
            {
                //hud坐标跟随
                Vector3 targetViewportPos = _gameCamera.WorldToViewportPoint(_followedTarget.position);
                Vector3 pos = _uiCamera.ViewportToWorldPoint(targetViewportPos);
                pos = transform.parent.InverseTransformPoint(pos);
                pos.z = 0f;
                transform.localPosition = pos;

                if (MyStructUIHUD.SkillButtonData.skillId!=0)
                {
                    //更新技能cd进度条
                    if (MyStructUIHUD.SkillButtonData.maxCDTime != 0 && MyStructUIHUD.SkillButtonData.currCDTime > 0)
                    {
                        MyStructUIHUD.SkillButtonData.currCDTime = Mathf.Max(0, MyStructUIHUD.SkillButtonData.currCDTime - Time.deltaTime);
                        float ratio = 1 - MyStructUIHUD.SkillButtonData.currCDTime / MyStructUIHUD.SkillButtonData.maxCDTime;
                        _mpBar.value = ratio;
                    }
                }
            }
        }
        #endregion

    }
}