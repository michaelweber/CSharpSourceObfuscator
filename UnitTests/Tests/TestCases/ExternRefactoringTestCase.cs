using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscatorUnitTests.Tests.TestCases
{
    class ExternRefactoringTestCase
    {
        [DllImport("vaultcli.dll")]
        public extern static Int32 VaultOpenVault(ref Guid vaultGuid, UInt32 offset, ref IntPtr vaultHandle);

        [DllImport("vaultcli.dll")]
        public extern static Int32 VaultCloseVault(ref IntPtr vaultHandle);

        [DllImport("vaultcli.dll")]
        public extern static Int32 VaultFree(ref IntPtr vaultHandle);

        [DllImport("vaultcli.dll")]
        public extern static Int32 VaultEnumerateVaults(Int32 offset, ref Int32 vaultCount, ref IntPtr vaultGuid);

        [DllImport("vaultcli.dll")]
        public extern static Int32 VaultEnumerateItems(IntPtr vaultHandle, Int32 chunkSize, ref Int32 vaultItemCount, ref IntPtr vaultItem);

        [DllImport("vaultcli.dll", EntryPoint = "VaultGetItem")]
        public extern static Int32 VaultGetItem_WIN8(IntPtr vaultHandle, ref Guid schemaId, IntPtr pResourceElement, IntPtr pIdentityElement, IntPtr pPackageSid, IntPtr zero, Int32 arg6, ref IntPtr passwordVaultPtr);

        [DllImport("vaultcli.dll", EntryPoint = "VaultGetItem")]
        public extern static Int32 VaultGetItem_WIN7(IntPtr vaultHandle, ref Guid schemaId, IntPtr pResourceElement, IntPtr pIdentityElement, IntPtr zero, Int32 arg5, ref IntPtr passwordVaultPtr);

        // [DllImport("Netapi32.dll")]
        // public extern static uint NetLocalGroupGetMembers([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string localgroupname, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, out IntPtr resumehandle);
        //
        // [DllImport("Netapi32.dll")]
        // public extern static int NetApiBufferFree(IntPtr Buffer);
        //
        // [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        // public static extern int WNetGetConnection(
        //     [MarshalAs(UnmanagedType.LPTStr)] string localName,
        //     [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
        //     ref int length);
        //
        // [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        // static extern bool ConvertSidToStringSid(IntPtr pSID, out IntPtr ptrSid);
        //
        // [DllImport("kernel32.dll")]
        // static extern IntPtr LocalFree(IntPtr hMem);
        //
        // [DllImport("advapi32.dll", SetLastError = true)]
        // static extern bool GetTokenInformation(
        //     IntPtr TokenHandle,
        //     TOKEN_INFORMATION_CLASS TokenInformationClass,
        //     IntPtr TokenInformation,
        //     int TokenInformationLength,
        //     out int ReturnLength);
        //
        // [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        // [return: MarshalAs(UnmanagedType.Bool)]
        // protected static extern bool LookupPrivilegeName(
        //     string lpSystemName,
        //     IntPtr lpLuid,
        //     System.Text.StringBuilder lpName,
        //     ref int cchName);
        //
        // [DllImport("wtsapi32.dll", SetLastError = true)]
        // static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);
        //
        // [DllImport("wtsapi32.dll")]
        // static extern void WTSCloseServer(IntPtr hServer);
        //
        // [DllImport("wtsapi32.dll", SetLastError = true)]
        // static extern Int32 WTSEnumerateSessions(
        //     IntPtr hServer,
        //     [MarshalAs(UnmanagedType.U4)] Int32 Reserved,
        //     [MarshalAs(UnmanagedType.U4)] Int32 Version,
        //     ref IntPtr ppSessionInfo,
        //     [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);
        //
        // [DllImport("wtsapi32.dll", SetLastError = true)]
        // static extern Int32 WTSEnumerateSessionsEx(
        //     IntPtr hServer,
        //     [MarshalAs(UnmanagedType.U4)] ref Int32 pLevel,
        //     [MarshalAs(UnmanagedType.U4)] Int32 Filter,
        //     ref IntPtr ppSessionInfo,
        //     [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);
        //
        // [DllImport("wtsapi32.dll")]
        // static extern void WTSFreeMemory(IntPtr pMemory);
        //
        // [DllImport("Wtsapi32.dll", SetLastError = true)]
        // static extern bool WTSQuerySessionInformation(
        //     IntPtr hServer,
        //     uint sessionId,
        //     WTS_INFO_CLASS wtsInfoClass,
        //     out IntPtr ppBuffer,
        //     out uint pBytesReturned
        // );
        //
        // [DllImport("iphlpapi.dll", SetLastError = true)]
        // public static extern uint GetExtendedTcpTable(
        //     IntPtr pTcpTable,
        //     ref uint dwOutBufLen,
        //     bool sort,
        //     int ipVersion,
        //     TCP_TABLE_CLASS tblClass,
        //     int reserved);
        //
        // [DllImport("advapi32.dll", SetLastError = true)]
        // public static extern uint I_QueryTagInformation(
        //     IntPtr Unknown,
        //     SC_SERVICE_TAG_QUERY_TYPE Type,
        //     ref SC_SERVICE_TAG_QUERY Query
        //     );
        //
        // [DllImport("iphlpapi.dll", SetLastError = true)]
        // public static extern uint GetExtendedUdpTable(
        //     IntPtr pUdpTable,
        //     ref uint dwOutBufLen,
        //     bool sort,
        //     int ipVersion,
        //     UDP_TABLE_CLASS tblClass,
        //     int reserved);
        //
        //
        //
        // [DllImport("secur32.dll", SetLastError = false)]
        // private static extern int LsaConnectUntrusted([Out] out IntPtr LsaHandle);
        //
        // [DllImport("secur32.dll", SetLastError = true)]
        // public static extern int LsaRegisterLogonProcess(LSA_STRING_IN LogonProcessName, out IntPtr LsaHandle, out ulong SecurityMode);
        //
        // [DllImport("secur32.dll", SetLastError = false)]
        // private static extern int LsaDeregisterLogonProcess([In] IntPtr LsaHandle);
        //
        // [DllImport("secur32.dll", SetLastError = false)]
        // public static extern int LsaLookupAuthenticationPackage([In] IntPtr LsaHandle, [In] ref LSA_STRING_IN PackageName, [Out] out int AuthenticationPackage);
        //
        // [DllImport("secur32.dll", SetLastError = false)]
        // private static extern int LsaCallAuthenticationPackage(IntPtr LsaHandle, int AuthenticationPackage, ref KERB_QUERY_TKT_CACHE_REQUEST ProtocolSubmitBuffer, int SubmitBufferLength, out IntPtr ProtocolReturnBuffer, out int ReturnBufferLength, out int ProtocolStatus);
        //
        // [DllImport("secur32.dll", EntryPoint = "LsaCallAuthenticationPackage", SetLastError = false)]
        // private static extern int LsaCallAuthenticationPackage_KERB_RETRIEVE_TKT(IntPtr LsaHandle, int AuthenticationPackage, ref KERB_RETRIEVE_TKT_REQUEST ProtocolSubmitBuffer, int SubmitBufferLength, out IntPtr ProtocolReturnBuffer, out int ReturnBufferLength, out int ProtocolStatus);
        //
        // [DllImport("secur32.dll", EntryPoint = "LsaCallAuthenticationPackage", SetLastError = false)]
        // private static extern int LsaCallAuthenticationPackage_KERB_RETRIEVE_TKT_UNI(IntPtr LsaHandle, int AuthenticationPackage, ref KERB_RETRIEVE_TKT_REQUEST_UNI ProtocolSubmitBuffer, int SubmitBufferLength, out IntPtr ProtocolReturnBuffer, out int ReturnBufferLength, out int ProtocolStatus);
        //
        // [DllImport("secur32.dll", SetLastError = false)]
        // private static extern uint LsaFreeReturnBuffer(IntPtr buffer);
        //
        // [DllImport("Secur32.dll", SetLastError = false)]
        // private static extern uint LsaEnumerateLogonSessions(out UInt64 LogonSessionCount, out IntPtr LogonSessionList);
        //
        // [DllImport("Secur32.dll", SetLastError = false)]
        // private static extern uint LsaGetLogonSessionData(IntPtr luid, out IntPtr ppLogonSessionData);
        //
        // // for GetSystem()
        // [DllImport("advapi32.dll", SetLastError = true)]
        // [return: MarshalAs(UnmanagedType.Bool)]
        // static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);
        //
        // [DllImport("advapi32.dll")]
        // public extern static bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);
        //
        // [DllImport("advapi32.dll", SetLastError = true)]
        // static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        //
        // [DllImport("advapi32.dll", SetLastError = true)]
        // static extern bool RevertToSelf();
        //
        // [DllImport("kernel32.dll", SetLastError = true)]
        // [return: MarshalAs(UnmanagedType.Bool)]
        // static extern bool CloseHandle(IntPtr hObject);
        //
        // [DllImport("kernel32.dll")]
        // static extern IntPtr LocalAlloc(uint uFlags, uint uBytes);
        //
        // [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        // public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        //
        // [DllImport("IpHlpApi.dll")]
        // [return: MarshalAs(UnmanagedType.U4)]
        // internal static extern int GetIpNetTable(IntPtr pIpNetTable, [MarshalAs(UnmanagedType.U4)]ref int pdwSize, bool bOrder);
        //
        // [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        // internal static extern int FreeMibTable(IntPtr plpNetTable);
        //
        // [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        // static extern bool LookupAccountSid(
        //   string lpSystemName,
        //   [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
        //   StringBuilder lpName,
        //   ref uint cchName,
        //   StringBuilder ReferencedDomainName,
        //   ref uint cchReferencedDomainName,
        //   out SID_NAME_USE peUse);
        //
        // public const uint SE_GROUP_LOGON_ID = 0xC0000000; // from winnt.h
        // public const int TokenGroups = 2; // from TOKEN_INFORMATION_CLASS
        // enum TOKEN_INFORMATION_CLASS
        // {
        //     TokenUser = 1,
        //     TokenGroups,
        //     TokenPrivileges,
        //     TokenOwner,
        //     TokenPrimaryGroup,
        //     TokenDefaultDacl,
        //     TokenSource,
        //     TokenType,
        //     TokenImpersonationLevel,
        //     TokenStatistics,
        //     TokenRestrictedSids,
        //     TokenSessionId,
        //     TokenGroupsAndPrivileges,
        //     TokenSessionReference,
        //     TokenSandBoxInert,
        //     TokenAuditPolicy,
        //     TokenOrigin
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct SID_AND_ATTRIBUTES
        // {
        //     public IntPtr Sid;
        //     public uint Attributes;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct TOKEN_GROUPS
        // {
        //     public int GroupCount;
        //     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        //     public SID_AND_ATTRIBUTES[] Groups;
        // };
        //
        // protected struct TOKEN_PRIVILEGES
        // {
        //     public UInt32 PrivilegeCount;
        //     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 35)]
        //     public LUID_AND_ATTRIBUTES[] Privileges;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // protected struct LUID_AND_ATTRIBUTES
        // {
        //     public LUID Luid;
        //     public UInt32 Attributes;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // protected struct LUID
        // {
        //     public uint LowPart;
        //     public int HighPart;
        // }
        //
        // [Flags]
        // public enum FirewallProfiles : int
        // {
        //     DOMAIN = 1,
        //     PRIVATE = 2,
        //     PUBLIC = 4,
        //     ALL = 2147483647
        // }
        //
        // [Flags]
        // public enum LuidAttributes : uint
        // {
        //     DISABLED = 0x00000000,
        //     SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001,
        //     SE_PRIVILEGE_ENABLED = 0x00000002,
        //     SE_PRIVILEGE_REMOVED = 0x00000004,
        //     SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000
        // }
        //
        // enum SID_NAME_USE
        // {
        //     SidTypeUser = 1,
        //     SidTypeGroup,
        //     SidTypeDomain,
        //     SidTypeAlias,
        //     SidTypeWellKnownGroup,
        //     SidTypeDeletedAccount,
        //     SidTypeInvalid,
        //     SidTypeUnknown,
        //     SidTypeComputer
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct WTS_SESSION_INFO
        // {
        //     public Int32 SessionID;
        //
        //     [MarshalAs(UnmanagedType.LPStr)]
        //     public String pWinStationName;
        //
        //     public WTS_CONNECTSTATE_CLASS State;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct WTS_SESSION_INFO_1
        // {
        //     public Int32 ExecEnvId;
        //
        //     public WTS_CONNECTSTATE_CLASS State;
        //
        //     public Int32 SessionID;
        //
        //     [MarshalAs(UnmanagedType.LPStr)]
        //     public String pSessionName;
        //
        //     [MarshalAs(UnmanagedType.LPStr)]
        //     public String pHostName;
        //
        //     [MarshalAs(UnmanagedType.LPStr)]
        //     public String pUserName;
        //
        //     [MarshalAs(UnmanagedType.LPStr)]
        //     public String pDomainName;
        //
        //     [MarshalAs(UnmanagedType.LPStr)]
        //     public String pFarmName;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct WTS_CLIENT_ADDRESS
        // {
        //     public uint AddressFamily;
        //     [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //     public byte[] Address;
        // }
        //
        // public enum WTS_CONNECTSTATE_CLASS
        // {
        //     Active,
        //     Connected,
        //     ConnectQuery,
        //     Shadow,
        //     Disconnected,
        //     Idle,
        //     Listen,
        //     Reset,
        //     Down,
        //     Init
        // }
        //
        // public enum WTS_INFO_CLASS
        // {
        //     WTSInitialProgram = 0,
        //     WTSApplicationName = 1,
        //     WTSWorkingDirectory = 2,
        //     WTSOEMId = 3,
        //     WTSSessionId = 4,
        //     WTSUserName = 5,
        //     WTSWinStationName = 6,
        //     WTSDomainName = 7,
        //     WTSConnectState = 8,
        //     WTSClientBuildNumber = 9,
        //     WTSClientName = 10,
        //     WTSClientDirectory = 11,
        //     WTSClientProductId = 12,
        //     WTSClientHardwareId = 13,
        //     WTSClientAddress = 14,
        //     WTSClientDisplay = 15,
        //     WTSClientProtocolType = 16,
        //     WTSIdleTime = 17,
        //     WTSLogonTime = 18,
        //     WTSIncomingBytes = 19,
        //     WTSOutgoingBytes = 20,
        //     WTSIncomingFrames = 21,
        //     WTSOutgoingFrames = 22,
        //     WTSClientInfo = 23,
        //     WTSSessionInfo = 24,
        //     WTSSessionInfoEx = 25,
        //     WTSConfigInfo = 26,
        //     WTSValidationInfo = 27,
        //     WTSSessionAddressV4 = 28,
        //     WTSIsRemoteSession = 29
        // }
        //
        // public enum TCP_TABLE_CLASS : int
        // {
        //     TCP_TABLE_BASIC_LISTENER,
        //     TCP_TABLE_BASIC_CONNECTIONS,
        //     TCP_TABLE_BASIC_ALL,
        //     TCP_TABLE_OWNER_PID_LISTENER,
        //     TCP_TABLE_OWNER_PID_CONNECTIONS,
        //     TCP_TABLE_OWNER_PID_ALL,
        //     TCP_TABLE_OWNER_MODULE_LISTENER,
        //     TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        //     TCP_TABLE_OWNER_MODULE_ALL
        // }
        //
        // public enum UDP_TABLE_CLASS : int
        // {
        //     UDP_TABLE_BASIC,
        //     UDP_TABLE_OWNER_PID,
        //     UDP_TABLE_OWNER_MODULE
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct SC_SERVICE_TAG_QUERY
        // {
        //     public uint ProcessId;
        //     public uint ServiceTag;
        //     public uint Unknown;
        //     public IntPtr Buffer;
        // }
        //
        // public enum SC_SERVICE_TAG_QUERY_TYPE
        // {
        //     ServiceNameFromTagInformation = 1,
        //     ServiceNamesReferencingModuleInformation = 2,
        //     ServiceNameTagMappingInformation = 3
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_TCPTABLE_OWNER_MODULE
        // {
        //     public uint NumEntries;
        //     MIB_TCPROW_OWNER_MODULE Table;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_TCPROW_OWNER_MODULE
        // {
        //     public readonly MIB_TCP_STATE State;
        //     public readonly uint LocalAddr;
        //     private readonly byte LocalPort1;
        //     private readonly byte LocalPort2;
        //     private readonly byte LocalPort3;
        //     private readonly byte LocalPort4;
        //     public readonly uint RemoteAddr;
        //     private readonly byte RemotePort1;
        //     private readonly byte RemotePort2;
        //     private readonly byte RemotePort3;
        //     private readonly byte RemotePort4;
        //     public readonly uint OwningPid;
        //     public readonly UInt64 CreateTimestamp;
        //     public readonly UInt64 OwningModuleInfo0;
        //     public readonly UInt64 OwningModuleInfo1;
        //     public readonly UInt64 OwningModuleInfo2;
        //     public readonly UInt64 OwningModuleInfo3;
        //     public readonly UInt64 OwningModuleInfo4;
        //     public readonly UInt64 OwningModuleInfo5;
        //     public readonly UInt64 OwningModuleInfo6;
        //     public readonly UInt64 OwningModuleInfo7;
        //     public readonly UInt64 OwningModuleInfo8;
        //     public readonly UInt64 OwningModuleInfo9;
        //     public readonly UInt64 OwningModuleInfo10;
        //     public readonly UInt64 OwningModuleInfo11;
        //     public readonly UInt64 OwningModuleInfo12;
        //     public readonly UInt64 OwningModuleInfo13;
        //     public readonly UInt64 OwningModuleInfo14;
        //     public readonly UInt64 OwningModuleInfo15;
        //
        //
        //     public ushort LocalPort
        //     {
        //         get
        //         {
        //             return BitConverter.ToUInt16(
        //                 new byte[2] { LocalPort2, LocalPort1 }, 0);
        //         }
        //     }
        //
        //     public IPAddress LocalAddress
        //     {
        //         get { return new IPAddress(LocalAddr); }
        //     }
        //
        //     public IPAddress RemoteAddress
        //     {
        //         get { return new IPAddress(RemoteAddr); }
        //     }
        //
        //     public ushort RemotePort
        //     {
        //         get
        //         {
        //             return BitConverter.ToUInt16(
        //                 new byte[2] { RemotePort2, RemotePort1 }, 0);
        //         }
        //     }
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_UDPTABLE_OWNER_MODULE
        // {
        //     public uint NumEntries;
        //     MIB_UDPROW_OWNER_MODULE Table;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_UDPROW_OWNER_MODULE
        // {
        //     public readonly uint LocalAddr;
        //     private readonly byte LocalPort1;
        //     private readonly byte LocalPort2;
        //     private readonly byte LocalPort3;
        //     private readonly byte LocalPort4;
        //     public readonly uint OwningPid;
        //     public readonly UInt64 CreateTimestamp;
        //     public readonly UInt32 SpecificPortBind_Flags;
        //     // public readonly UInt32 Flags;
        //     public readonly UInt64 OwningModuleInfo0;
        //     public readonly UInt64 OwningModuleInfo1;
        //     public readonly UInt64 OwningModuleInfo2;
        //     public readonly UInt64 OwningModuleInfo3;
        //     public readonly UInt64 OwningModuleInfo4;
        //     public readonly UInt64 OwningModuleInfo5;
        //     public readonly UInt64 OwningModuleInfo6;
        //     public readonly UInt64 OwningModuleInfo7;
        //     public readonly UInt64 OwningModuleInfo8;
        //     public readonly UInt64 OwningModuleInfo9;
        //     public readonly UInt64 OwningModuleInfo10;
        //     public readonly UInt64 OwningModuleInfo11;
        //     public readonly UInt64 OwningModuleInfo12;
        //     public readonly UInt64 OwningModuleInfo13;
        //     public readonly UInt64 OwningModuleInfo14;
        //     public readonly UInt64 OwningModuleInfo15;
        //
        //     public ushort LocalPort
        //     {
        //         get
        //         {
        //             return BitConverter.ToUInt16(
        //                 new byte[2] { LocalPort2, LocalPort1 }, 0);
        //         }
        //     }
        //
        //     public IPAddress LocalAddress
        //     {
        //         get { return new IPAddress(LocalAddr); }
        //     }
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_TCPROW_OWNER_PID
        // {
        //     public uint state;
        //     public uint localAddr;
        //     public byte localPort1;
        //     public byte localPort2;
        //     public byte localPort3;
        //     public byte localPort4;
        //     public uint remoteAddr;
        //     public byte remotePort1;
        //     public byte remotePort2;
        //     public byte remotePort3;
        //     public byte remotePort4;
        //     public int owningPid;
        //
        //     public ushort LocalPort
        //     {
        //         get
        //         {
        //             return BitConverter.ToUInt16(
        //                 new byte[2] { localPort2, localPort1 }, 0);
        //         }
        //     }
        //
        //     public IPAddress LocalAddress
        //     {
        //         get { return new IPAddress(localAddr); }
        //     }
        //
        //     public IPAddress RemoteAddress
        //     {
        //         get { return new IPAddress(remoteAddr); }
        //     }
        //
        //     public ushort RemotePort
        //     {
        //         get
        //         {
        //             return BitConverter.ToUInt16(
        //                 new byte[2] { remotePort2, remotePort1 }, 0);
        //         }
        //     }
        //
        //     public MIB_TCP_STATE State
        //     {
        //         get { return (MIB_TCP_STATE)state; }
        //     }
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_UDPROW_OWNER_PID
        // {
        //     public uint localAddr;
        //     //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        //     public byte localPort1;
        //     public byte localPort2;
        //     public byte localPort3;
        //     public byte localPort4;
        //     public int owningPid;
        //
        //     public ushort LocalPort
        //     {
        //         get
        //         {
        //             return BitConverter.ToUInt16(
        //                 new byte[2] { localPort2, localPort1 }, 0);
        //         }
        //     }
        //
        //     public IPAddress LocalAddress
        //     {
        //         get { return new IPAddress(localAddr); }
        //     }
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_TCPTABLE_OWNER_PID
        // {
        //     public uint dwNumEntries;
        //     MIB_TCPROW_OWNER_PID table;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct MIB_UDPTABLE_OWNER_PID
        // {
        //     public uint dwNumEntries;
        //     MIB_TCPROW_OWNER_PID table;
        // }
        //
        // public enum MIB_TCP_STATE
        // {
        //     CLOSED = 1,
        //     LISTEN = 2,
        //     SYN_SENT = 3,
        //     SYN_RCVD = 4,
        //     ESTAB = 5,
        //     FIN_WAIT1 = 6,
        //     FIN_WAIT2 = 7,
        //     CLOSE_WAIT = 8,
        //     CLOSING = 9,
        //     LAST_ACK = 10,
        //     TIME_WAIT = 11,
        //     DELETE_TCB = 12
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct LSA_STRING_IN
        // {
        //     public UInt16 Length;
        //     public UInt16 MaximumLength;
        //     public string Buffer;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct LSA_STRING_OUT
        // {
        //     public UInt16 Length;
        //     public UInt16 MaximumLength;
        //     public IntPtr Buffer;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct UNICODE_STRING : IDisposable
        // {
        //     public ushort Length;
        //     public ushort MaximumLength;
        //     public IntPtr buffer;
        //
        //     public UNICODE_STRING(string s)
        //     {
        //         Length = (ushort)(s.Length * 2);
        //         MaximumLength = (ushort)(Length + 2);
        //         buffer = Marshal.StringToHGlobalUni(s);
        //     }
        //
        //     public void Dispose()
        //     {
        //         Marshal.FreeHGlobal(buffer);
        //         buffer = IntPtr.Zero;
        //     }
        //
        //     public override string ToString()
        //     {
        //         return Marshal.PtrToStringUni(buffer);
        //     }
        // }
        //
        // public enum KERB_PROTOCOL_MESSAGE_TYPE : UInt32
        // {
        //     KerbDebugRequestMessage = 0,
        //     KerbQueryTicketCacheMessage = 1,
        //     KerbChangeMachinePasswordMessage = 2,
        //     KerbVerifyPacMessage = 3,
        //     KerbRetrieveTicketMessage = 4,
        //     KerbUpdateAddressesMessage = 5,
        //     KerbPurgeTicketCacheMessage = 6,
        //     KerbChangePasswordMessage = 7,
        //     KerbRetrieveEncodedTicketMessage = 8,
        //     KerbDecryptDataMessage = 9,
        //     KerbAddBindingCacheEntryMessage = 10,
        //     KerbSetPasswordMessage = 11,
        //     KerbSetPasswordExMessage = 12,
        //     KerbVerifyCredentialsMessage = 13,
        //     KerbQueryTicketCacheExMessage = 14,
        //     KerbPurgeTicketCacheExMessage = 15,
        //     KerbRefreshSmartcardCredentialsMessage = 16,
        //     KerbAddExtraCredentialsMessage = 17,
        //     KerbQuerySupplementalCredentialsMessage = 18,
        //     KerbTransferCredentialsMessage = 19,
        //     KerbQueryTicketCacheEx2Message = 20,
        //     KerbSubmitTicketMessage = 21,
        //     KerbAddExtraCredentialsExMessage = 22,
        //     KerbQueryKdcProxyCacheMessage = 23,
        //     KerbPurgeKdcProxyCacheMessage = 24,
        //     KerbQueryTicketCacheEx3Message = 25,
        //     KerbCleanupMachinePkinitCredsMessage = 26,
        //     KerbAddBindingCacheEntryExMessage = 27,
        //     KerbQueryBindingCacheMessage = 28,
        //     KerbPurgeBindingCacheMessage = 29,
        //     KerbQueryDomainExtendedPoliciesMessage = 30,
        //     KerbQueryS4U2ProxyCacheMessage = 31
        // }
        //
        // public enum KERB_ENCRYPTION_TYPE : UInt32
        // {
        //     reserved0 = 0,
        //     des_cbc_crc = 1,
        //     des_cbc_md4 = 2,
        //     des_cbc_md5 = 3,
        //     reserved1 = 4,
        //     des3_cbc_md5 = 5,
        //     reserved2 = 6,
        //     des3_cbc_sha1 = 7,
        //     dsaWithSHA1_CmsOID = 9,
        //     md5WithRSAEncryption_CmsOID = 10,
        //     sha1WithRSAEncryption_CmsOID = 11,
        //     rc2CBC_EnvOID = 12,
        //     rsaEncryption_EnvOID = 13,
        //     rsaES_OAEP_ENV_OID = 14,
        //     des_ede3_cbc_Env_OID = 15,
        //     des3_cbc_sha1_kd = 16,
        //     aes128_cts_hmac_sha1_96 = 17,
        //     aes256_cts_hmac_sha1_96 = 18,
        //     aes128_cts_hmac_sha256_128 = 19,
        //     aes256_cts_hmac_sha384_192 = 20,
        //     rc4_hmac = 23,
        //     rc4_hmac_exp = 24,
        //     camellia128_cts_cmac = 25,
        //     camellia256_cts_cmac = 26,
        //     subkey_keymaterial = 65
        // }
        //
        // [Flags]
        // private enum KERB_CACHE_OPTIONS : UInt64
        // {
        //     KERB_RETRIEVE_TICKET_DEFAULT = 0x0,
        //     KERB_RETRIEVE_TICKET_DONT_USE_CACHE = 0x1,
        //     KERB_RETRIEVE_TICKET_USE_CACHE_ONLY = 0x2,
        //     KERB_RETRIEVE_TICKET_USE_CREDHANDLE = 0x4,
        //     KERB_RETRIEVE_TICKET_AS_KERB_CRED = 0x8,
        //     KERB_RETRIEVE_TICKET_WITH_SEC_CRED = 0x10,
        //     KERB_RETRIEVE_TICKET_CACHE_TICKET = 0x20,
        //     KERB_RETRIEVE_TICKET_MAX_LIFETIME = 0x40,
        // }
        //
        // // TODO: double check these flags...
        // // https://docs.microsoft.com/en-us/windows/desktop/api/ntsecapi/ns-ntsecapi-_kerb_external_ticket
        // [Flags]
        // public enum KERB_TICKET_FLAGS : UInt32
        // {
        //     reserved = 2147483648,
        //     forwardable = 0x40000000,
        //     forwarded = 0x20000000,
        //     proxiable = 0x10000000,
        //     proxy = 0x08000000,
        //     may_postdate = 0x04000000,
        //     postdated = 0x02000000,
        //     invalid = 0x01000000,
        //     renewable = 0x00800000,
        //     initial = 0x00400000,
        //     pre_authent = 0x00200000,
        //     hw_authent = 0x00100000,
        //     ok_as_delegate = 0x00040000,
        //     name_canonicalize = 0x00010000,
        //     //cname_in_pa_data = 0x00040000,
        //     enc_pa_rep = 0x00010000,
        //     reserved1 = 0x00000001
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct SECURITY_HANDLE
        // {
        //     public IntPtr LowPart;
        //     public IntPtr HighPart;
        //     public SECURITY_HANDLE(int dummy)
        //     {
        //         LowPart = HighPart = IntPtr.Zero;
        //     }
        // };
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct KERB_TICKET_CACHE_INFO
        // {
        //     public LSA_STRING_OUT ServerName;
        //     public LSA_STRING_OUT RealmName;
        //     public Int64 StartTime;
        //     public Int64 EndTime;
        //     public Int64 RenewTime;
        //     public Int32 EncryptionType;
        //     public UInt32 TicketFlags;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // public struct KERB_TICKET_CACHE_INFO_EX
        // {
        //     public LSA_STRING_OUT ClientName;
        //     public LSA_STRING_OUT ClientRealm;
        //     public LSA_STRING_OUT ServerName;
        //     public LSA_STRING_OUT ServerRealm;
        //     public Int64 StartTime;
        //     public Int64 EndTime;
        //     public Int64 RenewTime;
        //     public Int32 EncryptionType;
        //     public UInt32 TicketFlags;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_QUERY_TKT_CACHE_RESPONSE
        // {
        //     public KERB_PROTOCOL_MESSAGE_TYPE MessageType;
        //     public int CountOfTickets;
        //     // public KERB_TICKET_CACHE_INFO[] Tickets;
        //     public IntPtr Tickets;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_QUERY_TKT_CACHE_EX_RESPONSE
        // {
        //     public KERB_PROTOCOL_MESSAGE_TYPE MessageType;
        //     public int CountOfTickets;
        //     // public KERB_TICKET_CACHE_INFO[] Tickets;
        //     public IntPtr Tickets;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_QUERY_TKT_CACHE_REQUEST
        // {
        //     public KERB_PROTOCOL_MESSAGE_TYPE MessageType;
        //     public LUID LogonId;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_RETRIEVE_TKT_REQUEST
        // {
        //     public KERB_PROTOCOL_MESSAGE_TYPE MessageType;
        //     public LUID LogonId;
        //     public LSA_STRING_IN TargetName;
        //     public UInt64 TicketFlags;
        //     public KERB_CACHE_OPTIONS CacheOptions;
        //     public Int64 EncryptionType;
        //     public SECURITY_HANDLE CredentialsHandle;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_RETRIEVE_TKT_REQUEST_UNI
        // {
        //     public KERB_PROTOCOL_MESSAGE_TYPE MessageType;
        //     public LUID LogonId;
        //     public UNICODE_STRING TargetName;
        //     public UInt64 TicketFlags;
        //     public KERB_CACHE_OPTIONS CacheOptions;
        //     public Int64 EncryptionType;
        //     public SECURITY_HANDLE CredentialsHandle;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_CRYPTO_KEY
        // {
        //     public Int32 KeyType;
        //     public Int32 Length;
        //     public IntPtr Value;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_EXTERNAL_NAME
        // {
        //     public Int16 NameType;
        //     public UInt16 NameCount;
        //     public LSA_STRING_OUT Names;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_EXTERNAL_TICKET
        // {
        //     public IntPtr ServiceName;
        //     public IntPtr TargetName;
        //     public IntPtr ClientName;
        //     public LSA_STRING_OUT DomainName;
        //     public LSA_STRING_OUT TargetDomainName;
        //     public LSA_STRING_OUT AltTargetDomainName;
        //     public KERB_CRYPTO_KEY SessionKey;
        //     public UInt32 TicketFlags;
        //     public UInt32 Flags;
        //     public Int64 KeyExpirationTime;
        //     public Int64 StartTime;
        //     public Int64 EndTime;
        //     public Int64 RenewUntil;
        //     public Int64 TimeSkew;
        //     public Int32 EncodedTicketSize;
        //     public IntPtr EncodedTicket;
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct KERB_RETRIEVE_TKT_RESPONSE
        // {
        //     public KERB_EXTERNAL_TICKET Ticket;
        // }
        //
        // private enum SECURITY_LOGON_TYPE : uint
        // {
        //     Interactive = 2,        // logging on interactively.
        //     Network,                // logging using a network.
        //     Batch,                  // logon for a batch process.
        //     Service,                // logon for a service account.
        //     Proxy,                  // Not supported.
        //     Unlock,                 // Tattempt to unlock a workstation.
        //     NetworkCleartext,       // network logon with cleartext credentials
        //     NewCredentials,         // caller can clone its current token and specify new credentials for outbound connections
        //     RemoteInteractive,      // terminal server session that is both remote and interactive
        //     CachedInteractive,      // attempt to use the cached credentials without going out across the network
        //     CachedRemoteInteractive,// same as RemoteInteractive, except used internally for auditing purposes
        //     CachedUnlock            // attempt to unlock a workstation
        // }
        //
        // [StructLayout(LayoutKind.Sequential)]
        // private struct SECURITY_LOGON_SESSION_DATA
        // {
        //     public UInt32 Size;
        //     public LUID LoginID;
        //     public LSA_STRING_OUT Username;
        //     public LSA_STRING_OUT LoginDomain;
        //     public LSA_STRING_OUT AuthenticationPackage;
        //     public UInt32 LogonType;
        //     public UInt32 Session;
        //     public IntPtr PSiD;
        //     public UInt64 LoginTime;
        //     public LSA_STRING_OUT LogonServer;
        //     public LSA_STRING_OUT DnsDomainName;
        //     public LSA_STRING_OUT Upn;
        // }
        //
        // public const int MAXLEN_PHYSADDR = 8;
        // public const int ERROR_SUCCESS = 0;
        // public const int ERROR_INSUFFICIENT_BUFFER = 122;
        //
        // [StructLayout(LayoutKind.Sequential)]
        // internal struct MIB_IPNETROW
        // {
        //     [MarshalAs(UnmanagedType.U4)]
        //     public int dwIndex;
        //     [MarshalAs(UnmanagedType.U4)]
        //     public int dwPhysAddrLen;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac0;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac1;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac2;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac3;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac4;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac5;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac6;
        //     [MarshalAs(UnmanagedType.U1)]
        //     public byte mac7;
        //     [MarshalAs(UnmanagedType.U4)]
        //     public int dwAddr;
        //     [MarshalAs(UnmanagedType.U4)]
        //     public int dwType;
        // }
        //
        // public enum ArpEntryType
        // {
        //     Other = 1,
        //     Invalid = 2,
        //     Dynamic = 3,
        //     Static = 4,
        // }
    }
}
