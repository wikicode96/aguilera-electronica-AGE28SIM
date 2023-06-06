using System.Diagnostics;
using System.Net.Sockets;

namespace Aguilera_connect
{
    internal class Aguilera
    {
        /*
	    * SYN | SYN | STX | LON | DIR | ACK |        CRC     | EOT | NUL
        *  22 |  22 |  2  |  5  |  1  |  6  | crc >> 8 | crc |  4  |  0  <-- ACK
        *  
        * SYN | SYN | STX | LON | DIR | NAK | ERR |       CRC      | EOT | NUL
        *  22 |  22 |  2  |  6  |  1  | 21  |  n  | crc >> 8 | crc |  4  |  0  <-- NAK
        */

        private byte[] ACK = { 22, 22, 2, 5, 1, 6, 81, 163, 4, 0 };
        private byte[] NAK = { 22, 22, 2, 6, 1, 21, 7, 175, 128, 4, 0 };
        private int port;
        private string ip;
        private int DIR;
        private TcpClient client;
        private NetworkStream stream;

        private static readonly ushort[] crc16Table = new ushort[]
        {
            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
            0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
            0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
            0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
            0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
            0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
            0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
            0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
            0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
            0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
            0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
            0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
            0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
            0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
            0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
            0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
            0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
            0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
            0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
            0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
            0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
            0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
            0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
            0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
            0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
            0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
            0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
            0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
            0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
            0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
            0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
            0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
        };

        // Constructor
        public Aguilera(int port, string ip, int dir)
        {
			this.port = port;
			this.ip = ip;
            this.DIR = dir;

            this.client = new TcpClient(this.ip, this.port);
            this.stream = client.GetStream();
        }

        // Obtiene el valor CRC16 del bloque de datos recibido
        private static ushort GetCRC16(byte[] buf)
        {
            ushort crc = 0;

            foreach (ushort val in buf)
            {
                crc = (ushort)(crc16Table[(crc >> 8) & 0xff] ^ (crc << 8) ^ val);
            }
            return crc;
        }
        public void SetPointsStatusForChannel(int ACC, int CAN, int EQU, int PTO)
        {
            /* Package format (Decimal)
            * 
            * ACC can have four status.
            * 0 = Replenish
            * 1 = Activate
            * 2 = Disconnect
            * 3 = Connect
            * 
	        *                 |                     DATA                     |
	        * SYN | SYN | STX |  LON | DIR | CMD | SUB CMD | ACC | CAN | EQU | PTO |        CRC     | EOT | NUL
	        *  22 |  22 |  2  |  10  |  1  | 'H' |   'F'   |  n  |  n  |  n  |  n  | crc >> 8 | crc |  4  |  0  <-- POINTS CONTROL
	        */
            byte[] package = new byte[]
            {
                10, (byte) this.DIR, (byte)'H', (byte)'F', (byte) ACC, (byte) CAN, (byte) EQU, (byte) PTO
            };

            ushort crc = GetCRC16(package);
            //Debug.WriteLine("CRC: {0:X}", crc);

            byte[] message = new byte[]
            {
                22, 22, 2, 10, (byte) this.DIR, (byte)'H', (byte)'F', (byte) ACC, (byte) CAN, (byte) EQU, (byte) PTO, 0, 0, 4, 0
            };

            message[11] = (byte)(crc >> 8);
            message[12] = (byte)crc;

            this.stream.Write(message, 0, message.Length);

            byte[] response = new byte[11];
            this.stream.Read(response, 0, response.Length);

            if (response[3] == ACK[3] && response[5] == ACK[5])
            {
                Debug.WriteLine("OK, ACK");
            }
            else if (response[5] == NAK[5])
            {
                Debug.WriteLine("Invalid command in Aguilera.SetPointsStatusForChannel()");
            }
        }
        public void SetPointsStatusForZones(int ACC, int ZON1, int ZON2, int PTO)
        {
            /* Package format (Decimal)
            * 
            * ACC can have four status.
            * 0 = Replenish
            * 1 = Activate
            * 2 = Disconnect
            * 3 = Connect
            * 
	        *                 |                     DATA                     |
	        * SYN | SYN | STX |  LON | DIR | CMD | SUB CMD | ACC | ZON | PTO |        CRC     | EOT | NUL
	        *  22 |  22 |  2  |  10  |  1  | 'H' |   'U'   |  n  | 0 n |  n  | crc >> 8 | crc |  4  |  0  <-- POINTS CONTROL
	        */
            byte[] package = new byte[]
            {
                10, (byte) this.DIR, (byte)'H', (byte)'U', (byte) ACC, (byte) ZON1, (byte) ZON2, (byte) PTO
            };

            ushort crc = GetCRC16(package);
            //Debug.WriteLine("CRC: {0:X}", crc);

            byte[] message = new byte[]
            {
                22, 22, 2, 10, (byte) this.DIR, (byte)'H', (byte)'U', (byte) ACC, (byte) ZON1, (byte) ZON2, (byte) PTO, 0, 0, 4, 0
            };

            message[11] = (byte)(crc >> 8);
            message[12] = (byte)crc;

            this.stream.Write(message, 0, message.Length);

            byte[] response = new byte[11];
            this.stream.Read(response, 0, response.Length);

            if (response[3] == ACK[3] && response[5] == ACK[5])
            {
                Debug.WriteLine("OK, ACK");
            }
            else if (response[5] == NAK[5])
            {
                Debug.WriteLine("Invalid command in Aguilera.SetPointsStatusForZones()");
            } 
        }
        public void RepositionSystem(int TIP)
        {
            /* Package format (Decimal)
             * 
             * TIP can have four status.
             * 0 = Silence
             * 1 = Reposition
             * 2 = Rearm (Disconected Points will hold this status)
             * 4 = Reset (All OK)
             * 
             *                 |               DATA              |
             * SYN | SYN | STX | LON | DIR | CMD | SUB CMD | TIP |        CRC     | EOT | NUL
             *  22 |  22 |  2  |  7  |  1  | 'H' |   'X'   |  n  | crc >> 8 | crc |  4  |  0  <-- REPOSITION
             */

            byte[] package = new byte[]
            {
                7, (byte) this.DIR, (byte)'H', (byte)'X', (byte) TIP
            };

            ushort crc = GetCRC16(package);
            //Debug.WriteLine("CRC: {0:X}", crc);

            byte[] message = new byte[]
            {
                22, 22, 2, 7, (byte) this.DIR, (byte)'H',(byte)'X', (byte) TIP, 0, 0, 4, 0
            };

            message[8] = (byte)(crc >> 8);
            message[9] = (byte)crc;

            this.stream.Write(message, 0, message.Length);

            byte[] response = new byte[10];
            this.stream.Read(response, 0, response.Length);

            Debug.WriteLine("Reposition of System");
        }
        public List<Zone> GetZonesStatus()
        {
            /* Package format (Decimal)
            * 
	        *                 |         DATA              |
	        * SYN | SYN | STX | LON | DIR | CMD | SUB CMD |        CRC     | EOT | NUL
	        *  22 |  22 |  2  |  6  |  1  | 'V' |   'R'   | crc >> 8 | crc |  4  |  0  <-- RESET
	        *  22 |  22 |  2  |  6  |  1  | 'V' |   ENQ   | crc >> 8 | crc |  4  |  0  <-- POLLING
	        *  22 |  22 |  2  |  6  |  1  | 'V' |   CAN   | crc >> 8 | crc |  4  |  0  <-- CAN
	        */

            List<Zone> zones = new List<Zone>();
            List<byte[]> records = new List<byte[]>();

            Reset(this.stream);
            records = Polling(this.stream);

            if (records.Count > 0)
            {
                CAN(this.stream); // Confirm  the records to Aguilera

                foreach (byte[] record in records)
                {
                    // Zone's Constructor = int number, int totalPoints, int preAlarmPoints, int alarmPoints, int activatePoints, int faultPoints, int desconectPoints, int testingPoints
                    Zone zone = new Zone(record[1], record[2], record[4], record[5], record[6], record[7], record[8], record[9], record[10]);

                    /*
                     *  'z' |  ZON  | EST | PTOT | PPRE | PALA | PACT | PAVE | PDES | PTST
                     *  122 | 39 15 |  0  |  13  |   0  |   0  |   0  |   0  |   0  |   0  <-- Sytem's Zone
                     *  
                     *  If this zone is the last record, it have information about System Relays. Always is Zone 39 15
                     */

                    zones.Add(zone);
                }
            }
            return zones;

            void Reset(NetworkStream stream)
            {
                byte[] package = new byte[]
                {
                    6, (byte) this.DIR, (byte)'V', (byte)'R'
                };

                ushort crc = GetCRC16(package);
                //Debug.WriteLine("CRC: {0:X}", crc);

                byte[] message = new byte[]
                {
                    22, 22, 2, 6, (byte) this.DIR, (byte)'V',(byte)'R', 0, 0, 4, 0
                };

                message[7] = (byte)(crc >> 8);
                message[8] = (byte)crc;

                stream.Write(message, 0, message.Length);

                byte[] response = new byte[10];
                stream.Read(response, 0, response.Length);

                //Debug.WriteLine("Reset sent");
            }
            List<byte[]> Polling(NetworkStream stream)
            {
                byte[] package = new byte[]
                {
                    6, (byte) this.DIR, (byte)'V', 5
                };

                ushort crc = GetCRC16(package);
                //Debug.WriteLine("CRC: {0:X}", crc);

                byte[] message = new byte[]
                {
                    22, 22, 2, 6, (byte) this.DIR, (byte)'V', 5, 0, 0, 4, 0
                };

                message[7] = (byte)(crc >> 8);
                message[8] = (byte)crc;

                stream.Write(message, 0, message.Length);

                byte[] response = new byte[176]; // Need to be 176. Read documentation
                int bytesRead = stream.Read(response, 0, response.Length);
                Array.Resize(ref response, bytesRead);

                if (response[3] == ACK[3] && response[5] == ACK[5])
                {
                    Debug.WriteLine("All zones OK");
                    return records;
                }
                else
                {
                    return GetRecords(response, 122, 11);
                }
            }
            void CAN(NetworkStream stream)
            {
                byte[] package = new byte[]
                {
                    6, (byte) this.DIR, (byte)'V', 24
                };

                ushort crc = GetCRC16(package);
                //Debug.WriteLine("CRC: {0:X}", crc);

                byte[] message = new byte[]
                {
                    22, 22, 2, 6, (byte) this.DIR, (byte)'V', 24, 0, 0, 4, 0
                };

                message[7] = (byte)(crc >> 8);
                message[8] = (byte)crc;

                stream.Write(message, 0, message.Length);

                byte[] zRecord = new byte[10];
                stream.Read(zRecord, 0, zRecord.Length);
            }
        }
        public List<Point> GetPointsStatus(int ZON1, int ZON2, int PTO = 0, int EST1 = 0xFF, int EST2 = 0xFF)
        {
            /*
            * SYN | SYN | STX | LON | DIR | CMD | SUB CMD | ZON | PTO |   EST  |        CRC     | EOT | NUL
            *  22 |  22 |  2  |  11 |  1  | 'V' |   'P'   |  n  |  0  | 0xFFFF | crc >> 8 | crc |  4  |  0  <-- REPOSITION
            */

            List<Point> points = new List<Point>();

            byte[] package = new byte[]
            {
                11, (byte)this.DIR, (byte)'V', (byte)'P', (byte)ZON1, (byte)ZON2, (byte)PTO, (byte)0xFF, (byte)0xFF
            };

            ushort crc = GetCRC16(package);
            //Debug.WriteLine("CRC: {0:X}", crc);

            byte[] message = new byte[]
            {
                    22, 22, 2, 11, (byte)this.DIR, (byte)'V', (byte)'P', (byte)ZON1, (byte)ZON2, (byte)PTO, (byte)0xFF, (byte)0xFF, 0, 0, 4, 0
            };

            message[12] = (byte)(crc >> 8);
            message[13] = (byte)crc;

            stream.Write(message, 0, message.Length);

            byte[] response = new byte[1024];
            int bytesRead = stream.Read(response, 0, response.Length);
            Array.Resize(ref response, bytesRead);

            if (response[3] == ACK[3] && response[5] == ACK[5])
            {
                Debug.WriteLine("All points in " + ZON1 + " " + ZON2 + " OK");
                return points;
            }
            else
            {
                List<byte[]> records  = GetRecords(response , 112, 7);
                
                foreach (byte[] record in records)
                {
                    Point point = new Point(record[1], record[2], record[3], record[4], record[5], record[6]);
                    points.Add(point);
                }
                return points;
            }
        }
        public void Close()
        {
            this.stream.Close();
            this.client.Close();
        }
        List<byte[]> GetRecords(byte[] response, int firstByte, int packageLength)
        {
            List<byte[]> records = new List<byte[]>();

            byte[] record = new byte[packageLength];
            int counter = -1;

            foreach (byte b in response)
            {
                if (b == firstByte)
                {
                    counter = 0;
                }
                if (counter != -1)
                {
                    record[counter] = b;
                    counter++;
                }
                if (counter == packageLength)
                {
                    byte[] newRecord = new byte[packageLength];
                    Array.Copy(record, newRecord, record.Length);
                    records.Add(newRecord);

                    counter = -1;
                }
                Debug.Write(b + " ");
            }
            Debug.Write("\n");
            return records;
        }
    }
}
