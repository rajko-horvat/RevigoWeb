/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for revigo
CREATE DATABASE IF NOT EXISTS `revigo` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `revigo`;

-- Dumping structure for table revigo.stats
CREATE TABLE IF NOT EXISTS `stats` (
  `ID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `DateTimeTicks` bigint(20) NOT NULL,
  `JobID` bigint(20) NOT NULL DEFAULT '0',
  `RequestSource` int(11) NOT NULL DEFAULT '0',
  `Cutoff` int(11) NOT NULL DEFAULT '0',
  `ValueType` int(11) NOT NULL DEFAULT '0',
  `SpeciesTaxon` int(11) NOT NULL DEFAULT '0',
  `Measure` int(11) NOT NULL DEFAULT '0',
  `RemoveObsolete` int(11) NOT NULL DEFAULT '0',
  `BiologicalProcess` bigint(20) NOT NULL DEFAULT '0',
  `CellularComponent` bigint(20) NOT NULL DEFAULT '0',
  `MolecularFunction` bigint(20) NOT NULL DEFAULT '0',
  `ExecTicks` bigint(20) NOT NULL DEFAULT '0',
  `Count` bigint(20) NOT NULL DEFAULT '0',
  `NSCount` double DEFAULT '0',
  `Table_BP` double DEFAULT '0',
  `Table_CC` double DEFAULT '0',
  `Table_MF` double DEFAULT '0',
  `Scatterplot_BP` double DEFAULT '0',
  `Scatterplot_CC` double DEFAULT '0',
  `Scatterplot_MF` double DEFAULT '0',
  `Scatterplot3D_BP` double DEFAULT '0',
  `Scatterplot3D_CC` double DEFAULT '0',
  `Scatterplot3D_MF` double DEFAULT '0',
  `Cytoscape_BP` double DEFAULT '0',
  `Cytoscape_CC` double DEFAULT '0',
  `Cytoscape_MF` double DEFAULT '0',
  `TreeMap_BP` double DEFAULT '0',
  `TreeMap_CC` double DEFAULT '0',
  `TreeMap_MF` double DEFAULT '0',
  `TagClouds` double DEFAULT '0',
  `SimMat_BP` double DEFAULT '0',
  `SimMat_CC` double DEFAULT '0',
  `SimMat_MF` double DEFAULT '0',
  PRIMARY KEY (`ID`),
  KEY `IDX_DateTimeTicks` (`DateTimeTicks`),
  KEY `IDX_Cutoff` (`DateTimeTicks`,`Cutoff`),
  KEY `IDX_ValueType` (`DateTimeTicks`,`ValueType`),
  KEY `IDX_SpeciesTaxon` (`DateTimeTicks`,`SpeciesTaxon`),
  KEY `IDX_Measure` (`DateTimeTicks`,`Measure`),
  KEY `IDX_RemoveObsolete` (`DateTimeTicks`,`RemoveObsolete`),
  KEY `IDX_RequestSource` (`DateTimeTicks`,`RequestSource`),
  KEY `IDX_DateTimeTicks_JobID` (`DateTimeTicks`,`JobID`),
  KEY `IDX_Agreggate` (`DateTimeTicks`,`Cutoff`,`ValueType`,`SpeciesTaxon`,`Measure`,`RemoveObsolete`,`RequestSource`,`NSCount`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=260551 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

-- Dumping structure for table revigo.stats_h
CREATE TABLE IF NOT EXISTS `stats_h` (
  `ID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `DateTimeTicks` bigint(20) NOT NULL,
  `RequestSource` int(11) NOT NULL DEFAULT '0',
  `Cutoff` int(11) NOT NULL DEFAULT '0',
  `ValueType` int(11) NOT NULL DEFAULT '0',
  `SpeciesTaxon` int(11) NOT NULL DEFAULT '0',
  `Measure` int(11) NOT NULL DEFAULT '0',
  `RemoveObsolete` int(11) NOT NULL DEFAULT '0',
  `BiologicalProcess` bigint(20) NOT NULL DEFAULT '0',
  `CellularComponent` bigint(20) NOT NULL DEFAULT '0',
  `MolecularFunction` bigint(20) NOT NULL DEFAULT '0',
  `ExecTicks` bigint(20) NOT NULL DEFAULT '0',
  `Count` bigint(20) NOT NULL DEFAULT '0',
  `NSCount` double DEFAULT '0',
  `Table_BP` double DEFAULT '0',
  `Table_CC` double DEFAULT '0',
  `Table_MF` double DEFAULT '0',
  `Scatterplot_BP` double DEFAULT '0',
  `Scatterplot_CC` double DEFAULT '0',
  `Scatterplot_MF` double DEFAULT '0',
  `Scatterplot3D_BP` double DEFAULT '0',
  `Scatterplot3D_CC` double DEFAULT '0',
  `Scatterplot3D_MF` double DEFAULT '0',
  `Cytoscape_BP` double DEFAULT '0',
  `Cytoscape_CC` double DEFAULT '0',
  `Cytoscape_MF` double DEFAULT '0',
  `TreeMap_BP` double DEFAULT '0',
  `TreeMap_CC` double DEFAULT '0',
  `TreeMap_MF` double DEFAULT '0',
  `TagClouds` double DEFAULT '0',
  `SimMat_BP` double DEFAULT '0',
  `SimMat_CC` double DEFAULT '0',
  `SimMat_MF` double DEFAULT '0',
  PRIMARY KEY (`ID`) USING BTREE,
  KEY `IDX_DateTimeTicks` (`DateTimeTicks`) USING BTREE,
  KEY `IDX_Cutoff` (`DateTimeTicks`,`Cutoff`) USING BTREE,
  KEY `IDX_ValueType` (`DateTimeTicks`,`ValueType`) USING BTREE,
  KEY `IDX_SpeciesTaxon` (`DateTimeTicks`,`SpeciesTaxon`) USING BTREE,
  KEY `IDX_Measure` (`DateTimeTicks`,`Measure`) USING BTREE,
  KEY `IDX_RemoveObsolete` (`DateTimeTicks`,`RemoveObsolete`) USING BTREE,
  KEY `IDX_RequestSource` (`DateTimeTicks`,`RequestSource`),
  KEY `IDX_Agreggate` (`DateTimeTicks`,`Cutoff`,`ValueType`,`SpeciesTaxon`,`Measure`,`RemoveObsolete`,`RequestSource`,`NSCount`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=58775 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
