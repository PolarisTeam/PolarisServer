/*
-- Table structure for table `players`
*/

CREATE TABLE IF NOT EXISTS `players` (
  `PlayerID` int(11) NOT NULL AUTO_INCREMENT,
  `Username` longtext,
  `Password` longtext,
  `Nickname` longtext,
  `SettingsINI` longtext,
  PRIMARY KEY (`PlayerID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=10000000 ;

/*
-- Table structure for table `characters`
*/


/* TODO: Check the auto increment for characters. */

CREATE TABLE IF NOT EXISTS `characters` (
  `CharacterID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext,
  `LooksBinary` longblob,
  `JobsBinary` longblob,
  `Player_PlayerID` int(11) DEFAULT NULL,
  PRIMARY KEY (`CharacterID`),
  KEY `IX_Player_PlayerID` (`Player_PlayerID`) USING HASH
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;


/*
-- Table structure for table `serverinfoes`
*/

CREATE TABLE IF NOT EXISTS `serverinfoes` (
  `info` varchar(255) CHARACTER SET utf8 NOT NULL,
  `setting` longtext,
  PRIMARY KEY (`info`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*
-- Constraints for table `characters`
*/

ALTER TABLE `characters`
  ADD CONSTRAINT `FK_Characters_Players_Player_PlayerID` FOREIGN KEY (`Player_PlayerID`) REFERENCES `players` (`PlayerID`);